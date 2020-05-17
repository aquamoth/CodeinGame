const MAX_COLLISIONS = 2;

enum PacType {
    ROCK = 0,
    PAPER = 1,
    SCISSORS = 2
}

type Pellet = {
    pos: Point,
    value: number
}

type Graph = {[hash: number]: GraphNode};

interface GraphNode {
    pos: Point,
    neighbours: GraphNode[]
}

class Point {
    public x: number;
    public y: number;

    constructor(x: number, y: number) {
        this.x = x;
        this.y = y;
    }

    public add(x: number, y: number) : Point {
        return new Point(
            this.x + x, 
            this.y + y
        );
    }

    public equals(p: Point) { 
        if(!p) return false;
        return this.x === p.x && this.y === p.y; 
    }

    public getHash(width: number) { return this.x + this.y * width; }

    toString() {
        return `${this.x} ${this.y}`;
    }
}

class GameArea {
    public width: number;
    public height: number;
    private rows: string[] = [];
    private graph: Graph;

    constructor(width: number, height: number) {
        this.width = width;
        this.height = height;
    }
    
    public addRow(row: string) {
        this.rows.push(row);
    }

    public build() {
        // console.error('Building node map');
        this.graph = {};
        
        for (let y = 0; y < this.height; y++) {
            const row = this.rows[y];

            for(let x = 0; x < this.width; x++) {
                if (row[x] === ' ') {
                    const pos = new Point(x, y);
                    this.graph[pos.getHash(this.width)] = { pos, neighbours: [] };
                }
            }
        }

        // console.error('Attaching nodes');
        for (let hash of Object.keys(this.graph)) {
            const node = this.graph[+hash];
            
            let next: GraphNode;
            next = this.getNode(node.pos.add(0, -1));
            if (next) node.neighbours.push(next);

            next = this.getNode(node.pos.add(-1, 0));
            if (next) node.neighbours.push(next);

            next = this.getNode(node.pos.add(0, 1));
            if (next) node.neighbours.push(next);

            next = this.getNode(node.pos.add(1, 0));
            if (next) node.neighbours.push(next);
        }
    }

    public getGraph() { return this.graph; }

    public getRandomPoint() {
        let pos: Point;
        do{
            let x = Math.round(Math.random() * map.width);
            let y = Math.round(Math.random() * map.height);
            pos = new Point(x, y);
        } while(!map.getNode(pos));
        return pos;
    }

    public getNode(pos: Point) {
        pos = this.normalize(pos);
        const hash = pos.getHash(this.width);
        return this.graph.hasOwnProperty(hash) ? this.graph[hash] : null;
    }

    public distance(from: Point, to: Point) {
        return Math.abs(to.x-from.x) % (this.width/2) 
            + Math.abs(to.y-from.y);
    }

    private normalize(pos: Point) {
        let x = pos.x;
        while(x<0) x+= this.width;
        while(x>=this.width) x-= this.width;

        let y = pos.y;
        while(y<0) y+= this.height;
        while(y>=this.height) y-= this.height;

        return new Point(x, y);
    }
}




type PacState = { 
    pacId: number, 
    mine: boolean,
    pos: Point,
    typeId: PacType,
    speedTurnsLeft: number,
    abilityCooldown: number,
}


class Pacman {
    public readonly id: number;
    private currentState: PacState;

    private previous: Point;
    private collisionTurns: number;
    public avoidanceTurns: number;

    public target: Point;

    constructor(id: number){
        this.id = id;
        this.currentState = null;

        this.previous = new Point(0,0);
        this.target = null;
        this.avoidanceTurns = 0;
    }

    public awaitingCommand(): boolean {
        return this.currentState !== null;
    }

    public clearState() {
        this.currentState = null;
    }

    public clearIfDead() {
        if(this.currentState === null && this.target !== null) {
            console.error(`Pac ${this.id} is DEAD! Removing target.`);
            this.target = null;
        }
    }

    public setState(state: PacState) {
        if(this.currentState !== null)
            throw "Pac should not set state twice!";
    
        this.currentState = state;
    }

    public getLocation() {
        return this.currentState.pos;
    }

    public getType() {
        return this.currentState.typeId;
    }

    public isColliding() {
        const current = this.currentState.pos;
        const hasMoved = (this.previous.x !== current.x) || (this.previous.y !== current.y);
        this.collisionTurns = hasMoved ? 0 : this.collisionTurns + 1;
        
        return this.collisionTurns > MAX_COLLISIONS;
    }

    public hasAbility() {
        return this.currentState.abilityCooldown === 0;
    }
}






interface CostNode extends GraphNode {
    cost: number;
}

interface DijkstraNode extends CostNode {
    path: number[];
    distance: number;
    visited: boolean;
}

class Dijkstra {
    readonly graph: {[hash:number]: DijkstraNode};
    readonly width: number;
    readonly debug: boolean;
    
    private from: Point;
    private player: Point;
    private playerHash: number;
    private unvisited: DijkstraNode[];

    constructor(graph: {[hash:number]: CostNode}, width: number, from: Point, player?: Point, debug?: boolean) {
        this.debug = debug || false;

        this.graph = {};
        Object.keys(graph)
            .forEach(hash => {
                this.graph[+hash] = {...graph[+hash], path: undefined, distance: undefined,visited:undefined};
            });

        this.width = width;
        this.reset(from, player || from);
    }

    private reset(from: Point, player: Point) {
        Object.keys(this.graph)
            .map(hash => this.graph[+hash])
            .forEach(node => {
                node.path = undefined;
                node.distance = Number.MAX_SAFE_INTEGER;
                node.visited = false;
            });

        const fromHash = from.getHash(this.width);
        const fromNode = this.graph[fromHash];
        fromNode.path=[fromHash];
        fromNode.distance=0;
        
        this.player = player;
        this.playerHash = player.getHash(this.width);
        this.from = from;
        this.unvisited = [fromNode];

        //if(this.debug) console.error(`reset graph: (${from}), (${player})`);
    }

    public pathTo(to: Point){

        const hash = to.getHash(this.width);
        const toNode = this.graph[hash];
        if(!toNode/*this.graph.hasOwnProperty(hash)*/)
            return null;

        if(toNode.path)
            return toNode.path;

        if(this.debug) console.error(`Calculating path to (${toNode.pos})`);
        while(this.unvisited.length>0) {
            const info = this.unvisited.map(node => ({ x: node.pos.x, y: node.pos.y, cost: node.cost, distance: node.distance }));
            //if(this.debug) console.error(`STACK: ${JSON.stringify(info)}`);

            // const currentNode = this.unvisited.shift();
            const currentNode = this.unvisited.reduce((prev, next) => { 
                    const nextCost = next.distance + map.distance(next.pos, to);
                    return prev.cost <= nextCost 
                        ? prev 
                        : { cost: nextCost, node: next }
                }, 
                {cost:Number.MAX_SAFE_INTEGER, node: <DijkstraNode>null}
            ).node;

            this.unvisited = this.unvisited.filter(node => node != currentNode);
            //if(this.debug) console.error(`  ${JSON.stringify(currentNode.pos)}`);

            currentNode.visited = true;

            if(!currentNode.path){
                console.error(`No path from ${this.from} to ${currentNode.pos}`)
            }
            else {
                //if(this.debug) console.error(`Testing ${currentNode.neighbours.length} neighbours for ${JSON.stringify(currentNode.pos)}`);
                for(const node of currentNode.neighbours){
                    const hash = node.pos.getHash(this.width);
                    const neighbour = this.graph[hash];
                    if(!neighbour.visited && hash !== this.playerHash) {
                        const tentativeDistance = currentNode.distance + neighbour.cost;
                        //if(this.debug) console.error(`  Neighbour ${ JSON.stringify({...neighbour, neighbours:undefined }) } distance ${tentativeDistance}`);
                        const isVisited = neighbour.path;
                        if(!isVisited || neighbour.distance > tentativeDistance) {
                            neighbour.distance = tentativeDistance;
                            neighbour.path = [...currentNode.path, hash];
                            //if(this.debug) console.error(`  Assigned ${isVisited?'improved ': ''}path ${neighbour.path}`);
                        }

                        if(!isVisited){
                            //if(this.debug) console.error(`  Pushing neighbour for calculation`);
                            this.unvisited.push(neighbour);
                        }
                    }
                }

                if(currentNode === toNode) {
                    //if(this.debug) console.error(`Reached sought node: ${currentNode.path}`);
                    return currentNode.path;
                }
            }
        }

        console.error(`Failed to find path from ${this.from} to ${toNode.pos}`);
        return null;
    }
}
























/**
 * Grab the pellets as fast as you can!
 **/

var inputs: string[] = readline().split(' ');
const map = loadGameAreaFromInput();

let myPacs: Pacman[] = undefined;









// game loop
while (true) {
    const startTime = Date.now();

    var inputs: string[] = readline().split(' ');
    const myScore: number = parseInt(inputs[0]);
    const opponentScore: number = parseInt(inputs[1]);

    const visiblePacStates = loadPacsFromInput();
    const myPacStates = visiblePacStates.filter(p=>p.mine);
    const opponentStates = visiblePacStates.filter(p=>!p.mine);

    const pellets = loadPelletsFromInput();




    if (myPacs === undefined)
        myPacs = myPacStates.map(p => new Pacman(p.pacId));

    for(const pac of myPacs)
        pac.clearState();

    for(const state of myPacStates)
        myPacs[state.pacId].setState(state);

    myPacs.forEach(pac=>pac.clearIfDead());

    const myLocations = Object.keys(myPacs)
        .map(id => myPacs[+id].getLocation())
        .filter(pos => !!pos);


    let commands = [];


    let pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    for(const pac of pendingPacs) {
        const pos = pac.getLocation();
        const opponentDists = opponentStates
            .map(o=>({o, dist: map.distance(o.pos, pos)}));
        
        if(pac.id === 0)
            console.error(`Opponents ${JSON.stringify(opponentDists.map(x=>({dist: x.dist, pos: x.o.pos})))}`)
        const closeOpponents = opponentDists
            .filter(o=>o.dist<3);

        if(closeOpponents.length>0)
        {
            console.error(`Pac ${pac.id} has ${closeOpponents.length} close opponents`);
            const closest = closeOpponents.sort(o=>o.dist)[0];
            const huntMode = (closest.o.typeId-pac.getType() + 3) % 3;
            if(huntMode === 2){
                console.error(`Pac ${pac.id} attacks opponent ${closest.o.pacId}`);
                commands.push(`MOVE ${pac.id} ${closest.o.pos} ATTACK ${closest.o.pacId}`);
                pac.clearState();
            }
            else if(closest.dist > 1 && pac.hasAbility){
                console.error(`Pac ${pac.id} prepares to attack ${closest.o.pacId}`);
                const newType = PacType[(closest.o.typeId + 1) % 3];
                commands.push(`SWITCH ${pac.id} ${newType}`);
                pac.clearState();
            }
            else {
                console.error(`Pac ${pac.id} SHOULD avoid ${closest.o.pacId}`);
            }
        }
    }

    //Avoid deadlocks
    pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    for (const pac of pendingPacs) {
        const command = considerAvoidanceRoutine(pac);
        if (command) {
            console.error(`Pac ${pac.id} avoiding deadlock`);
            commands.push(command);
            pac.clearState();
        }
    }


    //Keep moving towards target
    pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    let pacsWithTarget = pendingPacs.filter(pac => pac.target !== null);
    for (const pac of pacsWithTarget) {
        //Untarget if the target vanishes
        let targetIsPellet = pellets.some(p=>p.pos.equals(pac.target));
        if(!targetIsPellet) {
            console.error(`Pac ${pac.id} target is gone`);
            pac.target = null;
        }
        else {
            if (pac.hasAbility()) {
                console.error(`Speeding up ${pac.id}`);
                commands.push(`SPEED ${pac.id}`);
            } else {
                console.error(`Pac ${pac.id} continuing to target at (${pac.target})`);
                commands.push(`MOVE ${pac.id} ${pac.target} SUPER`);
            }
            pac.clearState();
        }
    }


    //Assign nearest pacs to super pellets
    pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    const untargetedSuperPellets = pellets
        .filter(pellet => pellet.value >= 10)
        .filter(pellet => !myPacs.some(pac => pellet.pos.equals(pac.target)));
    console.error(`${Date.now()-startTime}ms: Super pellets: ${JSON.stringify(untargetedSuperPellets.map(p=>p.pos))}`);

    let assignmentsLeft = Math.min(untargetedSuperPellets.length, pendingPacs.length);
    if(assignmentsLeft>0) {
        //console.error(`Assigning ${assignmentsLeft} super pellets among pacs ${pendingPacs.map(pac=>pac.id)}`);
        const costGraph = costGraphFrom(map.getGraph(), pellets, myLocations, map.width);

        let pelletPaths : {[id:number]: number[][]} = {};
        for(const pac of pendingPacs){
            const bfs = new Dijkstra(costGraph, map.width, pac.getLocation(), null, pac.id === 1);
            const paths = untargetedSuperPellets.map(pellet => bfs.pathTo(pellet.pos));
            pelletPaths[pac.id] = paths;
        }

        do{
            const nearest = Object.keys(pelletPaths)
                .map(pacId => ({
                    pacId: +pacId, 
                    distances: pelletPaths[+pacId]
                        .map(path=> path ? path.length: Number.MAX_SAFE_INTEGER)
                }))
                .map(x=> ({
                    pacId: x.pacId,
                    pellet: x.distances.reduce(
                        (best, dist, index) => best.dist < dist ? best : ({index, dist}), 
                        {index: -1, dist: Number.MAX_SAFE_INTEGER}
                    )
                }))
                .reduce(
                    (best, curr)=>best.pellet.dist < curr.pellet.dist ? best : curr, 
                    {pacId: -1, pellet: {index: -1, dist:Number.MAX_SAFE_INTEGER}}
                );
            //console.error(`nearest: ${JSON.stringify(nearest)}`);

            const pellet = untargetedSuperPellets[nearest.pellet.index];
            const pac = myPacs.filter(pac=>pac.id === nearest.pacId)[0];
            console.error(`Pac ${pac.id} targets closest super pellet at ${pellet.pos}, dist = ${nearest.pellet.dist}`);
            pac.target = pellet.pos;
            commands.push(`MOVE ${pac.id} ${pac.target} SUPER (NEW)`);
            pac.clearState();
            delete pelletPaths[pac.id];

            assignmentsLeft--;
        }
        while(assignmentsLeft>0);
    }



    pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    if(pendingPacs.length>0)
        console.error(`${Date.now()-startTime}ms: Assigning fallback route for ${pendingPacs.length} pacs`);
    for (const pac of pendingPacs) {
        //TODO: each pac should consider the best route to eat max pellets without crashing
        if (pellets.length>0){
            const pellet = pellets[(pac.id * 10) % pellets.length];
            console.error(`Moving ${pac.id} to pellet (${pellet.pos})`);
            pac.target = pellet.pos;
            commands.push(`MOVE ${pac.id} ${pellet.pos} SCAVANGE`);
        } else {
            console.error(`Pac ${pac.id} sees no opponents or pellets. Moving freely.`);
            let p = map.getRandomPoint();
            //pac.avoidanceTurns = 7;
            // const node = map.getNode(pac.location);
            // const p = node.neighbours[0].p;
            pac.target = p;
            commands.push(`MOVE ${pac.id} ${p} SEARCHING`);
        }
    }

    //console.error(`${Date.now()-startTime}ms: Submitting ${commands.length} commands`);
    console.log(commands.join(' | '));
}


function costGraphFrom(graph: Graph, pellets: Pellet[], myLocations: Point[], graphWidth: number){
    const costGraph: {[hash:number]: CostNode} = {};
    Object.keys(graph).map(hash => costGraph[+hash] = {...graph[+hash], cost:1});
    
    pellets.forEach(pellet => costGraph[pellet.pos.getHash(graphWidth)].cost -= pellet.value/4);
    myLocations.forEach(pos => costGraph[pos.getHash(graphWidth)].cost += 5);

    return costGraph;
}


function considerAvoidanceRoutine(pac:Pacman): string {
    if (pac.avoidanceTurns > 0) {
        console.error(`Pac ${pac.id} avoiding ${pac.avoidanceTurns} more turns.`);
        pac.avoidanceTurns--;
        return `MOVE ${pac.id} ${pac.target}`;
    } 

    if (pac.isColliding()) {
        console.error(`Pac ${pac.id} activating avoidance routine!`);
        let p = map.getRandomPoint();
        pac.target = p;
        pac.avoidanceTurns = 3;
        return `MOVE ${pac.id} ${p}`;
    }

    return undefined;
}




function loadGameAreaFromInput(){
    const width: number = parseInt(inputs[0]); // size of the grid
    const height: number = parseInt(inputs[1]); // top left corner is (x=0, y=0)
    let map = new GameArea(width, height);

    for (let y = 0; y < height; y++) {
        const row: string = readline(); // one line of the grid: space " " is floor, pound "#" is wall
        map.addRow(row);
        //console.error(row);
    }

    map.build();

    return map;
}

function loadPacsFromInput(){
    let pacs: PacState[] = [];
    
    const visiblePacCount: number = parseInt(readline()); // all your pacs and enemy pacs in sight
    for (let i = 0; i < visiblePacCount; i++) {
        var inputs: string[] = readline().split(' ');
        
        const pacId: number = parseInt(inputs[0]); // pac number (unique within a team)
        const mine: boolean = inputs[1] !== '0'; // true if this pac is yours
        const x: number = parseInt(inputs[2]); // position in the grid
        const y: number = parseInt(inputs[3]); // position in the grid
        const typeName: string = inputs[4]; // unused in wood leagues
        const speedTurnsLeft: number = parseInt(inputs[5]); // unused in wood leagues
        const abilityCooldown: number = parseInt(inputs[6]); // unused in wood leagues

        let pac: PacState = { 
            pacId, 
            mine, 
            pos: new Point(x, y),
            typeId: PacType[typeName], 
            speedTurnsLeft, 
            abilityCooldown 
        };
        // console.error(JSON.stringify(pac));
        pacs.push(pac);
    }

    return pacs;
}

function loadPelletsFromInput() {
    let pellets: Pellet[] = [];

    const visiblePelletCount: number = parseInt(readline()); // all pellets in sight
    for (let i = 0; i < visiblePelletCount; i++) {
        var inputs: string[] = readline().split(' ');
        const x: number = parseInt(inputs[0]);
        const y: number = parseInt(inputs[1]);
        const value: number = parseInt(inputs[2]); // amount of points this pellet is worth

        //console.error(`Pellet (${x}, ${y}): ${value} p`)
        pellets.push({
            pos: new Point(x, y), 
            value
        });
    }

    return pellets;
}
