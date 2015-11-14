/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

var deckP1 = [], deckP2 = [];

var n = parseInt(readline()); // the number of cards for player 1
for (var i = 0; i < n; i++) {
    var cardp1 = readline(); // the n cards of player 1
    deckP1.push(cardp1);
}
var m = parseInt(readline()); // the number of cards for player 2
for (var i = 0; i < m; i++) {
    var cardp2 = readline(); // the m cards of player 2
    deckP2.push(cardp2);
}

printErr('Deck p1: ' + deckP1);
printErr('Deck p2: ' + deckP2);

warpile1 = [];
warpile2 = [];

var gameRounds = 0;
while (deckP1.length > 0 && deckP2.length > 0) {
    gameRounds++;
    //printErr('');
    //printErr('Game round: ' + gameRounds);

    var c1 = deckP1.shift();
    warpile1.push(c1);

    var c2 = deckP2.shift();
    warpile2.push(c2);

    switch (winner(c1, c2)) {
        case -1:
            //printErr(c1 + ' <=> ' + c2 + '==> Player 1');
            deckP1 = deckP1.concat(warpile1).concat(warpile2);
            warpile1 = [];
            warpile2 = [];
            break;

        case 1:
            //printErr(c1 + ' <=> ' + c2 + '==> Player 2');
            deckP2 = deckP2.concat(warpile1).concat(warpile2);
            warpile1 = [];
            warpile2 = [];
            break;

        default:
            if (deckP1.length < 4 || deckP2.length < 4) {
                deckP1 = [];
                deckP2 = [];
            }
            else {
                warpile1 = warpile1.concat(deckP1.slice(0, 3));
                deckP1 = deckP1.slice(3);

                warpile2 = warpile2.concat(deckP2.slice(0, 3));
                deckP2 = deckP2.slice(3);

                gameRounds--;
            }
            break;
    }
}

if (deckP1.length > 0)
    print('1 ' + gameRounds);
else if (deckP2.length > 0)
    print('2 ' + gameRounds);
else
    print('PAT');




function winner(card1, card2) {
    var value = valueOf(card2) - valueOf(card1);
    return Math.sign(value);
}

function valueOf(card) {
    var value = card.slice(0, -1);
    switch (value) {
        case 'A': return 14;
        case 'K': return 13;
        case 'Q': return 12;
        case 'J': return 11;
        default:
            return parseInt(value);
    }
}