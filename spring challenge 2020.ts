const MAX_COLLISIONS = 2;

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
    typeId: string,
    speedTurnsLeft: number,
    abilityCooldown: number,
}


class Pacman {
    public readonly id: number;
    private currentState: PacState;

    private previous: Point[];
    public target: Point;
    public avoidanceTurns: number;

    constructor(id: number){
        this.id = id;
        this.currentState = null;

        this.previous = [];
        this.target = null;
        this.avoidanceTurns = 0;
    }

    public awaitingCommand(): boolean {
        return this.currentState !== null;
    }

    public clearState() {
        this.currentState = null;
    }

    public setState(state: PacState) {
        if(this.currentState !== null)
            throw "Pac should not set state twice!";
    
        this.currentState = state;
        this.previous.push(state.pos);
    }

    public getLocation() {
        return this.currentState.pos;
    }

    public isColliding() {
        if (this.previous.length <= MAX_COLLISIONS)
            return false;

        const last = this.previous.shift();

        const current = this.currentState.pos;
        return (last.x === current.x)
            && (last.y === current.y);
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
    
    private from: Point;
    private player: Point;
    private playerHash: number;
    private unvisited: DijkstraNode[];

    constructor(graph: {[hash:number]: CostNode}, width: number, from: Point, player?: Point) {
        
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
    }

    public pathTo(to: Point){
        const hash = to.getHash(this.width);
        const toNode = this.graph[hash];
        if(!toNode/*this.graph.hasOwnProperty(hash)*/)
            return null;

        if(toNode.path)
            return toNode.path;

        while(this.unvisited.length>0) {
            const currentNode = this.unvisited.pop();
            currentNode.visited = true;

            if(!currentNode.path){
                console.error(`No path from ${this.from} to ${currentNode.pos}`)
            }
            else {
                for(const node of currentNode.neighbours){
                    const hash = node.pos.getHash(this.width);
                    const neighbour = this.graph[hash];
                    if(!neighbour.visited && hash !== this.playerHash) {
                        const tentativeDistance = currentNode.distance + neighbour.cost;
                        if(!neighbour.path || neighbour.distance > tentativeDistance) {
                            neighbour.distance = tentativeDistance;
                            neighbour.path = [...currentNode.path, hash];
                        }

                        if(!neighbour.path)
                            this.unvisited.push(neighbour);
                    }
                }

                if(currentNode === toNode)
                    return currentNode.path;
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




    let commands = [];


    //TODO: Target opponents close by
        // else if(opponents.length>0) {
        //     const opponent = opponents[pac.pacId % opponents.length];
        //     console.error(`Pac ${pac.pacId} hunting opponent ${opponent.pacId} at (${opponent.location})`);
        //     pacState.target = opponent.location;
        //     commands.push(`MOVE ${pac.pacId} ${opponent.location}`);
        // }


    //Avoid deadlocks
    let pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    for (const pac of pendingPacs) {
        const command = considerAvoidanceRoutine(pac);
        if (command) {
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
                commands.push(`MOVE ${pac.id} ${pac.target}`);
            }

            pac.clearState();
        }
    }


    //Assign nearest pacs to super pellets
    pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    const untargetedSuperPellets = pellets
        .filter(pellet => pellet.value >= 10)
        .filter(pellet => !myPacs.some(pac => pellet.pos.equals(pac.target)));





    //Assign super pellet to closest pacs
    let assignmentsLeft = Math.min(untargetedSuperPellets.length, pendingPacs.length);
    if(assignmentsLeft>0) {
    // while (untargetedSuperPellets.length>0 && pendingPacs.length>0) {
        const costGraph = costGraphFrom(map.getGraph(), pellets);


        let pelletPaths : {[id:number]: object} = {};
        for(const pac of pendingPacs){
            const bfs = new Dijkstra(costGraph, map.width, pac.getLocation());
            const paths = untargetedSuperPellets.map(pellet => bfs.pathTo(pellet.pos));
            pelletPaths[pac.id] = paths;
        }

        do{

            //TODO: Assign closes pacs

            assignmentsLeft--;
        }
        while(assignmentsLeft>0);





        // console.error(`Assigning super pellet at (${pellet.pos}) to pac ${pac.id}`);
        // pac.target = pellet.pos;

        // const command = `MOVE ${pac.id} ${pac.target}`;
        // commands.push(command);
        // pac.clearState();
    }



    pendingPacs = myPacs.filter(pac=>pac.awaitingCommand());
    for (const pac of pendingPacs) {
        //TODO: each pac should consider the best route to eat max pellets without crashing
        if (pellets.length>0){
            const pellet = pellets[(pac.id * 10) % pellets.length];
            console.error(`Moving ${pac.id} to pellet (${pellet.pos})`);
            pac.target = pellet.pos;
            commands.push(`MOVE ${pac.id} ${pellet.pos}`);
        } else {
            console.error(`Pac ${pac.id} sees no opponents or pellets. Moving freely.`);
            let p = map.getRandomPoint();
            pac.avoidanceTurns = 7;
            // const node = map.getNode(pac.location);
            // const p = node.neighbours[0].p;
            pac.target = p;
            commands.push(`MOVE ${pac.id} ${p}`);
        }
    }

    console.log(commands.join(' | '));
}


function costGraphFrom(graph: Graph, pellets: Pellet[]){
    const costGraph: {[hash:number]: CostNode} = {};

    //TODO: Reduce cost for known pellets
    Object.keys(graph).map(hash => costGraph[+hash] = {...graph[+hash], cost:1});
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
        const typeId: string = inputs[4]; // unused in wood leagues
        const speedTurnsLeft: number = parseInt(inputs[5]); // unused in wood leagues
        const abilityCooldown: number = parseInt(inputs[6]); // unused in wood leagues

        let pac: PacState = { 
            pacId, 
            mine, 
            pos: new Point(x, y),
            typeId, 
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
