const SHIELD_RADIUS = 400;
const IMPACT_RANGE = 2 * SHIELD_RADIUS + 100;

let game = {
    numberOfLaps: parseInt(readline()),
    checkpoints: []
};

var numberOfCheckpoints = parseInt(readline());
for(var i=0; i < numberOfCheckpoints; i++) {
    game.checkpoints.push(vectorOf(readline()));
}

game.longestLeg = calculateLongestLeg(game.checkpoints);

printErr(`GAME ${game.numberOfLaps} laps * ${game.checkpoints.length} checkpoints. Longest leg is  ${game.longestLeg.index}.`);



let myRunner = { boost: true, pidI: 0 };
let myAttacker = { boost: true, pidI: 0 };
let opponents = [{}, {}];

while(true){
    readPosition(myRunner);
    readPosition(myAttacker);
    readPosition(opponents[0]);
    readPosition(opponents[1]);

    //printErr(JSON.stringify(opponents[0]));
    //printErr(JSON.stringify(opponents[1]));
    
    
    printErr('STEER RUNNER');
    print(steerRunner(game, myRunner, opponents));


    printErr('STEER ATTACKER');
    print(steerAttacker(game, myAttacker, opponents));
}

function needShield(pod, opponents){
    let myPos = nextPosOf(pod);
    
    let dist = distanceBetween(myPos, nextPosOf(opponents[0]));
    if (dist <= IMPACT_RANGE) {
        let impactSpeed = distanceBetween(pod.v, opponents[0].v);
        printErr(`Impact with O1 in speed ${impactSpeed}`);
        if (impactSpeed > 150)
            return true;
    }
    
    dist = distanceBetween(myPos, nextPosOf(opponents[1]));
    if (dist <= IMPACT_RANGE) {
        let impactSpeed = distanceBetween(pod.v, opponents[1].v);
        printErr(`Impact with O2 in speed ${impactSpeed}`);
        if (impactSpeed > 150)
            return true;
    }
    
    return false;    
}

function distanceBetween(v1, v2){
    return distanceOf(subtract(v1, v2));
}

function nextPosOf(pod){
    return add(pod.pos, pod.v);
}

function shouldAttack(game, pod, opponent) {
    if (distanceBetween(pod.pos, opponent.pos) > 4000)
        return false;

    let opponentA = (angleOf(opponent.v) + 360) % 360;
    let podA = (pod.pos.angle+360)%360;
    let deltaAngle = Math.abs(podA - ((opponentA+180)%360));
    
    printErr(`OA: ${opponentA}, PA: ${podA} => delta: ${deltaAngle}`);
    if (deltaAngle > 45)
        return false;
        
    return true;
}

function selectCheckpointToDefend(game, pod, opponent) {
    let opponentD = 0;

    //let opponentGlide = calcGlideTarget(opponent.pos, opponent.v);
    //let podGlide = calcGlideTarget(pod.pos, pod.v);
    //printErr(`me: ${JSON.stringify(podGlide)}, opponent: ${JSON.stringify(opponentGlide)}`);
    let last = opponent.pos; //opponentGlide;//opponent.pos;

    for (let i = 0; i < game.checkpoints.length; i++) {
        let index = (opponent.next + i) % game.checkpoints.length;
        let checkpoint = game.checkpoints[index];

        opponentD += distanceOf(subtract(checkpoint, last));
        let myCpD = distanceOf(subtract(checkpoint, pod.pos));
        //let myCpD = distanceOf(subtract(checkpoint, podGlide));

        //printErr(`${index}: ${myCpD}, ${opponentD}`);
        
        if (myCpD < 1200) {
            //printErr(`Attacker is defending checkpoint ${index}`);
            return checkpoint;
        }
        else if (myCpD < opponentD - 600) {
            //printErr(`Attacker heading to checkpoint ${index}`);
            return checkpoint;
        }
        
        last = checkpoint;

    }
    
    printErr('ERROR: selectCheckpointToDefend() found NO defendable checkpoint!!');
    return game.checkpoints[0];
}

function selectOpponentAhead(game, opponents){
    let one = opponents[0];
    let two = opponents[1];

    if (one.lap > two.lap)
        return one;
    else if (one.lap < two.lap)
        return two;
    else {
        let oneNext = (one.next+game.checkpoints.length-1) % game.checkpoints.length;
        let twoNext = (two.next+game.checkpoints.length-1) % game.checkpoints.length;
        //printErr(`Next [${one.next}, ${two.next}]. Alt [${oneNext}, ${twoNext}].`);
        
        if (oneNext > twoNext)
            return one;
        else if (oneNext < twoNext)
            return two;
        else {
            let checkpoint = game.checkpoints[one.next];
            one.cpD = distanceBetween(checkpoint, one.pos);
            two.cpD = distanceBetween(checkpoint, two.pos);
            return one.cpD < two.cpD ? one : two;
        }
    }
}

function calcGlideTarget(position, velocity){
    return add(position, multiply(velocity, 20/3));
}

function gotoTarget(game, pod, target) {
    printErr(`Targeting ${JSON.stringify(target)}`);

    let podAngle = (pod.pos.angle + 360) % 360;

    let S0 = pod.pos;
    let T = target;
    
    let S = calcGlideTarget(pod.pos, pod.v);

    let ST = subtract(T, S);
    
    let navError = distanceOf(ST);
    if (navError < 300){
        printErr(`${JSON.stringify(pod.pos)} -> ${JSON.stringify(target)}. S=${JSON.stringify(S)}. Error= ${navError}. Preparing for attack.`);
        return null;
    }
    
    
    
    
    let waypoint = add(S0, ST);
    
    let angleST = (angleOf(ST) + 360) % 360;
    let angleSTError = Math.abs(podAngle - angleST);

    let angleT = (angleOf(subtract(T, S0)) + 360) % 360;
    let angleTError = Math.abs(podAngle - angleT);
    
    let thrust = 0;
    if (angleTError > 90) {
        printErr(`Turning HARD; Target ${angleT} deg, WP ${angleST} deg, Pod ${podAngle} deg.`);
        thrust = 0;
    }
    else {
        let requiredThrust = distanceOf(multiply(ST, 3/20));

        if (pod.boost && requiredThrust >= 600 && angleSTError < 5) {
            thrust = 'BOOST';
            pod.boost = false;
        }
        else {
            thrust = Math.min(100, requiredThrust);
        }
    }
    
    return { x: waypoint.x, y: waypoint.y, thrust};
}

function steerRunner(game, pod, opponents){
    if (pod.next !== pod.last) {
        printErr('New checkpoint!');
        pod.pidI = 0;
    }
    //printErr('Running for checkpoint ' + pod.next + ' of lap ' + pod.lap);

    let checkpoint = game.checkpoints[pod.next];


    let angleNow = (angleOf(subtract(checkpoint, pod.pos)) + 360) % 360;
    let angle5 = (angleOf(subtract(checkpoint, add(pod.pos, multiply(pod.v, 5)))) + 360) % 360;
    let angleDiff = Math.abs(angle5 - angleNow);
    //printErr('Checkpoint angle diff in 5 turns: ' + angleDiff);

    let wp_vector = subtract(checkpoint, pod.pos);
    //printErr(`Checkpoint vector = ${JSON.stringify(wp_vector)}; ${angleOf(wp_vector)} degrees`);
    
    let target = { 
        dist: distanceOf(wp_vector), 
        angle: angleOf(wp_vector) 
    };

    let delta_angle = (target.angle - pod.pos.angle + 360) % 360;
    if (delta_angle > 180) delta_angle = 360 - delta_angle;

    let error = calculateError(pod, checkpoint);

    //printErr(`Runner ${JSON.stringify(pod.pos)}, error: ${error}`);
    //printErr(`Checkpoint ${JSON.stringify(checkpoint)} (${JSON.stringify(target)})`);
    //printErr(`Delta: ${delta_angle}, pidI: ${pod.pidI}`);
    
    let waypoint = checkpoint;
    let thrust;
    if (Math.abs(delta_angle) >= 90) {
        printErr('Turning hard without thrust');
        thrust = 0;
    }
    else {
        let absError = Math.abs(error);
        if (absError > 5 && absError < 75){
            printErr('Compensating for waypoint error: ' + absError);
            pod.pidI = error * 0.5;
            waypoint = rotate(waypoint, pod.pos, pod.pidI);
        }
        
        if (needShield(pod, opponents)) {
            printErr('Runner SHIELD up!');
            thrust = "SHIELD";
        }
        else if (target.dist < 2000 && Math.abs(delta_angle) > 30) {
            printErr('Slowing down to reach checkpoint');
            thrust = 50;
        }
        else if (angleDiff > 90) {
            printErr('Turning to next checkpoint');
            waypoint = game.checkpoints[(pod.next+1)%game.checkpoints.length];
            thrust = 25;
        }
        else if (pod.boost 
            && pod.next === game.longestLeg.index 
            && Math.abs(delta_angle) <= 10
            && Math.abs(pod.pidI) <= 10) {
            thrust = 'BOOST';
            pod.boost = false;
        }
        else
            thrust = 100;
    }

    return `${waypoint.x} ${waypoint.y} ${thrust}`
}

function steerAttacker(game, pod, opponents){

    let opponent = selectOpponentAhead(game, opponents);
    printErr(`Obstructing ${JSON.stringify(opponent.pos)}`);

    if (shouldAttack(game, pod, opponent)) {
    
        let offsetFactor = 300 / distanceOf(opponent.v);
        let offset = {
            x: Math.round(opponent.v.y * offsetFactor),
            y: Math.round(opponent.v.x * offsetFactor)
        };
        printErr('Attacking with offset: ' + JSON.stringify(offset));
        
        let wp = add(add(opponent.pos, opponent.v), offset);
        command = { x: wp.x, y: wp.y, thrust: 100 };
    }
    else {
        let checkpoint = selectCheckpointToDefend(game, myAttacker, opponent);
        printErr('Defending checkpoint: ' + JSON.stringify(checkpoint));

        command = gotoTarget(game, myAttacker, checkpoint);
        if (command === null){
            
            //prepare attack angle
            let trackingPos;
            if (game.checkpoints[opponent.next] === checkpoint) {
                trackingPos = opponent.pos;
                printErr('Tracking opponent: ' + JSON.stringify(trackingPos));
            }
            else {
                trackingPos = game.checkpoints[opponent.next];
                printErr('Turning to expected entrypoint: ' + JSON.stringify(trackingPos));
            }
            
            command = { x: trackingPos.x, y: trackingPos.y, thrust: 0};
        }
    }

    if (needShield(myAttacker, opponents)) {
        printErr('Attacker SHIELD up!');
        command.thrust = "SHIELD";
    }
    
    return `${command.x} ${command.y} ${command.thrust}`;
/*
    let opponent = opponents[0];
    printErr('attacking:' + JSON.stringify(opponent));
    
    let distance = distanceOf(subtract(opponent.pos, pod.pos));
    let time = distance / 50;
    let opponent_movement = multiply(opponent.v, time);
    let target = add(opponent.pos, opponent_movement);
    printErr(` - distance ${distance}. ${time} turns away.`);
    return `${Math.round(target.x)} ${Math.round(target.y)} ${100}`;
*/
}



function calculateLongestLeg(checkpoints){
    let longestLeg = { distance2: 0, index: 0 };
    
    for (let i=0; i< checkpoints.length; i++){
        var a = checkpoints[i];
        var b = checkpoints[(i + 1)%checkpoints.length];
        var distance2 = dist2Of(subtract(a,b));
        //printErr(`Distance2 to checkpoint ${i + 1} = ${distance2}`);
        if (distance2 > longestLeg.distance2)
            longestLeg = { distance2, index: i + 1 };
    }
    
    return longestLeg;
}

function readPosition(pod){
    let [x, y, vx, vy, angle, next] = readline().split(' ');    

    pod.pos = { x: +x, y: +y, angle: +angle };
    pod.v = { x: +vx, y: +vy };

    pod.last = pod.next;
    pod.next = +next;

    if (pod.next === 1 && pod.next !== pod.last) {
        pod.lap = (pod.lap || 0) + 1;
    }

    if (pod.last === undefined)
        pod.last = pod.next;
}


function vectorOf(s){
    let [x, y] = s.split(' ');
    return { 
        x: +x, 
        y: +y 
    };
}   


function distanceOf(v) {
    return Math.round(Math.sqrt(dist2Of(v)));
}

function angleOf(v) {
    return Math.round(Math.atan2(v.y, v.x) / Math.PI * 180);
}

function calculateError(pod, waypoint) {
    //https://www.mathsisfun.com/algebra/trig-solving-sss-triangles.html
    //cos(180 - error) = (b2 + a2 - c2) / (2 * a * b)
    var speed2 = dist2Of(pod.v);
    if (speed2 === 0)
        return 0;

    //printErr(`|${JSON.stringify(pod.v)}|=${Math.sqrt(speed2)}; ${angleOf(pod.v)} degrees`);
    
    let angle_to_waypoint = angleOf(subtract(waypoint, pod.pos));
    let speed_angle = angleOf(pod.v);
    
    var error = angle_to_waypoint - speed_angle;
    //var distance2 = dist2Of(subtract(pod.pos, waypoint));
    //var last_distance2 = dist2Of(subtract(subtract(pod.pos, pod.v), waypoint));
    //var error = 180 - 180 / Math.PI * Math.acos((distance2 + speed2 - last_distance2) / (2 * Math.sqrt(distance2 * speed2)));

    return error;
}

function dist2Of(v){
    return Math.pow(v.x, 2) + Math.pow(v.y, 2);
}

function add(a, b) {
    return {
        x: a.x + b.x,
        y: a.y + b.y
    };
}

function subtract(a, b) {
    return {
        x: a.x - b.x,
        y: a.y - b.y
    };
}

function multiply(v, amount){
    return {
        x: Math.round(v.x * amount),
        y: Math.round(v.y * amount)
    };
}

function rotate(point, center, degrees) {
    //printErr(`rotate ${JSON.stringify(point)}, ${degrees} degrees`);
    if (degrees === null)
        return point;
    
    let radians = degrees / 180 * Math.PI;
    let sinAngle = Math.sin(radians);
    let cosAngle = Math.cos(radians);
    let dx = point.x - center.x;
    let dy = point.y - center.y;

    return {
        x: Math.round(center.x + dx * cosAngle - dy * sinAngle),
        y: Math.round(center.y + dx * sinAngle + dy * cosAngle)
    }
}
