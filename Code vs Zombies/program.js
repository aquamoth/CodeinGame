/* global readline */
/* global printErr */
/**
 * Save humans, destroy zombies!
 **/


// game loop
while (true) {
    var inputs = readline().split(' ');
    var x = parseInt(inputs[0]);
    var y = parseInt(inputs[1]);


    var humans = [];
    var humanCount = parseInt(readline());
    for (var i = 0; i < humanCount; i++) {
        var inputs = readline().split(' ');
        humans.push({
            id: parseInt(inputs[0]),
            x: parseInt(inputs[1]),
            y: parseInt(inputs[2])
        });
    }
   
    
    var zombies = [];
    var zombieCount = parseInt(readline());
    for (var i = 0; i < zombieCount; i++) {
        var inputs = readline().split(' ');
        zombies.push({
            id: parseInt(inputs[0]),
            x: parseInt(inputs[1]),
            y: parseInt(inputs[2]),
            nextX: parseInt(inputs[3]),
            nextY: parseInt(inputs[4])
        });
    }


    var ash = {x:x, y:y};


    zombies.map(function(zombie){
        var distances = humans.map(function(human){
           var distance = distanceTo(zombie, human);
           return {human: human, distance: distance };
       });
       distances.sort(function(a,b){ 
           return compare(a.distance, b.distance); 
       });
       zombie.closest = distances[0];
    });

    
    for (var i =0;i<zombieCount; i++){
        var z = zombies[i]
        printErr(z.id + ': ' + z.x + ', ' + z.y + ' (' + z.nextX + ', ' + z.nextY + ' ). Closest: ' + z.closest.human.id + '. d=' + z.closest.distance);
    }


    var zombiesToTarget = zombies.filter(function(z){
        var survivingTurns = distanceTo(z, z.closest.human) / 400;
        var requiredTurns = distanceTo(ash, z.closest.human) / 1000 - 1;
        return requiredTurns < survivingTurns;
    })

    if (zombiesToTarget.length===0){
        print(ash.x + ' ' + ash.y);
    }
    else {
        zombiesToTarget.sort(function(z1,z2){return compare(z1.closest.distance, z2.closest.distance);});
        var target = zombiesToTarget[0];
        print(target.nextX + ' ' + target.nextY); // Your destination coordinates
    }

}

function distanceTo(p1, p2){
    var dx = p1.x-p2.x;
    var dy = p1.y-p2.y;
    var d = Math.sqrt(dx*dx + dy*dy);
    //printErr(p1.x + ',' + p1.y + ' <-> ' + p2.x + ',' + p2.y + ' = ' + d);
    return d;
}

function compare(v1, v2){
    return v1<v2 ?-1
        : (v2<v1 ? 1
        : 0);
}