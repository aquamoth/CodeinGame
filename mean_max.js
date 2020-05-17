const REAPER = 0;
const DESTROYER = 1;
const DOOF = 2;
const TANKER = 3;
const WRECK = 4;

const UNIT_TYPES = [
    {mass: 0.5, friction: 0.2},
    {mass: 1.5, friction: 0.3},
    {mass: 1.0, friction: 0.25}
];


let RANGE = 400 * 400;
let MAX_RANGE = 999999999;

let gameState;

while (true) {
    gameState = readState();    

    gameState[1].units
        .map(unit => targetFor(unit))
        .forEach(target => print(target));
}



function targetFor(unit){
    switch(unit.type){
        case REAPER: return reaperTarget(unit)
        case DESTROYER: return destroyerTarget(unit);
        case DOOF: return doofTarget(unit); 
        default:
            printErr('Unknown unit type!', unit.type);
            return null;
    }
}

function doofTarget(unit) {
    return "WAIT";
}

function destroyerTarget(unit){
    return "WAIT";
}

function reaperTarget(unit){
    printErr('Reaper', JSON.stringify(unit.xy), JSON.stringify(unit.speed));
    
    let wrecks = gameState[0].units.filter(u=>u.type === WRECK);
    if (wrecks.length === 0) {
        printErr('No wreck. Waiting');
        return "WAIT";
    }
        
    let target = selectClosest(wrecks, unit.xy);
    //if (target.d < RANGE) {
    //    printErr('Inside a wreck. Looting');
    //    return "WAIT";
    //}
        
    printErr('Targeting', JSON.stringify(target.wreck.xy));
    let delta = currentTarget(unit, target);
    
    printErr('Delta', JSON.stringify(delta), 'Dist:', lengthOf(delta));
    
    //vx += delta.x * power / mass / lengthOf(delta)
    //vy += delta.y * power / mass / lengthOf(delta)

    let thrust = Math.round(Math.min(300, lengthOf(delta) * unit.mass));
    let goto = { ... target.wreck.xy, thrust }
    printErr('Goto', JSON.stringify(goto));
    return `${goto.x} ${goto.y} ${goto.thrust}`;
}

function currentTarget(unit, target) {

    let friction = UNIT_TYPES[unit.type].friction;

    printErr('currentTarget', JSON.stringify({target: target.wreck.xy, unit: unit.xy, speed: unit.speed}));

    let coastingTo = restingPosition(unit.xy, unit.speed, friction);
    printErr('Coasting to', JSON.stringify(coastingTo));
    
    return subtract(target.wreck.xy, coastingTo);
}

function restingPosition(p0, v0, friction) {
    let v0Len = Math.sqrt(Math.pow(v0.x,2) + Math.pow(v0.y,2));
    let vtLen = 0.07; // ideally, this is 0, but that would take infinite amount of ticks
    if (v0Len <= vtLen) return p0;
    
    let fFriction = 1 - friction;
    let ticks = getBaseLog( fFriction, vtLen / v0Len );
    let factor = ( Math.pow( fFriction, ticks + 1 ) - 1 ) / ( fFriction - 1 );
    
    return {
      x: Math.round(p0.x + v0.x * factor),
      y: Math.round(p0.y + v0.y * factor)
    };
}

function getBaseLog(a, b){
    return Math.log(b) / Math.log(a);
}

function selectClosest(wrecks, from) {
    let closest = { wreck: null, d: MAX_RANGE, v: null };
    for (let wreck of wrecks) {
        let v = subtract(wreck.xy, from);
        let d = lengthOf(v);
        if (d < closest.d) 
            closest = { wreck, d, v };
    }
    return closest;
}

function subtract(a,b) {
    return {
        x: a.x-b.x, 
        y: a.y-b.y
        };
}

function distanceFor(a, b){
    return lengthOf(subtract(a,b));
}

function lengthOf(v){
    return Math.round(Math.sqrt(Math.pow(v.x, 2) + Math.pow(v.y, 2)));
}







function readState(){
    let players = [
        { units: [] },
        { score: parseInt(readline()), units: [] }, 
        { score: parseInt(readline()), units: [] }, 
        { score: parseInt(readline()), units: [] } 
    ];

    players[1].rage = parseInt(readline());
    players[2].rage = parseInt(readline());
    players[3].rage = parseInt(readline());

    readUnits(players);
    
    return players;
}

function readUnits(players){
    let unitCount = parseInt(readline());
    for (var i = 0; i < unitCount; i++) {
        let unit = readUnit();
        players[unit.player + 1].units.push(unit);
    }
}

function readUnit(){
    var inputs = readline().split(' ');
    return {
        id: parseInt(inputs[0]),
        type: parseInt(inputs[1]),
        player: parseInt(inputs[2]),
        mass: parseFloat(inputs[3]),
        radius: parseInt(inputs[4]),
        xy: {
            x: parseInt(inputs[5]),
            y: parseInt(inputs[6])
        },
        speed: {
            x: parseInt(inputs[7]),
            y: parseInt(inputs[8])
        },
        extra: parseInt(inputs[9]),
        extra2: parseInt(inputs[10])
    };
}
