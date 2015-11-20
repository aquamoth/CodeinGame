/* tremauxAlgorith found at https://github.com/primaryobjects/maze/blob/master/scripts/tremauxAlgorithm.js */

var inputs = readline().split(' ');
var R = parseInt(inputs[0]); // number of rows.
var C = parseInt(inputs[1]); // number of columns.
var A = parseInt(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

// game loop
while (true) {

    var inputs = readline().split(' ');
    var KR = parseInt(inputs[0]); // row where Kirk is located.
    var KC = parseInt(inputs[1]); // column where Kirk is located.
    for (var i = 0; i < R; i++) {
        var ROW = readline(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
    }

    // Write an action using print()
    // To debug: printErr('Debug messages...');

    print('RIGHT'); // Kirk's next move (UP DOWN LEFT or RIGHT).
}







var controller = new mazeController();

// Read any maze provided in the url querystring ?maze={start: {x: 3, y: 0}, end: {x: 6, y: 0}, width: 20, height: 10, map: '**** **** ...'}
commonManager.readCustomMaze();

// Include the desired starting maze in maze.html (mazes/big.js, mazes/little.js, etc.) or via querystring.
controller.init(maze);
controller.run();





var commonManager = {
    getParameterByName: function (name) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regexS = "[\\?&]" + name + "=([^&#]*)";
        var regex = new RegExp(regexS);
        var results = regex.exec(window.location.search);
        if (results == null) {
            return "";
        }
        else {
            return decodeURIComponent(results[1].replace(/\+/g, " "));
        }
    },

    readCustomMaze: function () {
        var param = commonManager.getParameterByName('maze');
        if (param != null && param.length > 0) {
            var data = JSON.parse(param);
            if (data.map != null && data.width > 0 && data.height > 0) {
                // Valid maze data.
                maze = data;

                return true;
            }
        }

        return false;
    },

    loadCustomMaze: function (filename) {
        if (filename != null) {
            // Load script instead.
            maze = null;
            this.loadScript(filename);

            return true;
        }

        return false;
    },

    loadScript: function (filename) {
        var head = document.head || document.getElementsByTagName('head')[0];
        var script = document.createElement("script");
        script.type = 'text/javascript';
        script.src = filename;
        head.insertBefore(script, head.firstChild);
    }
};

function mazeController() {
    this.canvas = null,
	this.context = null,
	this.maze = null,
	this.walker = null,
	this.algorithm = null,
	this.speed = null,

	this.init = function (maze) {
	    this.canvas = $('#imageView').get(0);
	    this.context = this.canvas.getContext('2d');

	    // Auto-adjust canvas size to fit window.
	    this.canvas.width = maze.width * 10;
	    this.canvas.height = maze.height * 10;

	    // Initialize speed.
	    this.speed = maze.speed == null ? 50 : maze.speed;

	    // Create maze.
	    this.maze = new mazeManager(this.context, maze);
	    this.maze.draw();

	    // Create walker at starting position.
	    this.walker = new walkerManager(this.context, this.maze);
	    this.walker.init();

	    // Initialize the maze algorithm.
	    this.algorithm = new searchAlgorithm(this.walker);
	},

	this.run = function () {
	    if (!this.algorithm.isDone()) {
	        this.algorithm.step();

	        window.setTimeout(function () {
	            controller.run();
	        }, this.speed);
	    }
	    else {
	        $('#btnGo').removeClass('disabled');
	        $('#btnGo').text('Go!');

	        // Clear map so we can draw the solution path.
	        this.walker.maze.draw(true);

	        // Draw the solution path.
	        this.algorithm.solve();
	    }
	}
};






function tremauxAlgorith(walker) {
    this.walker = walker,
	this.direction = 0,
	this.end = walker.maze.end,

	this.step = function () {
	    var startingDirection = this.direction;

	    while (!this.walker.move(this.direction)) {
	        // Hit a wall. Turn to the right.		
	        this.direction++;

	        if (this.direction > 3) {
	            this.direction = 0;
	        }

	        if (this.direction == startingDirection) {
	            // We've turned in a complete circle with no new path available. Time to backtrack.
	            while (!this.walker.move(this.direction, true)) {
	                // Hit a wall. Turn to the right.		
	                this.direction++;

	                if (this.direction > 3) {
	                    this.direction = 0;
	                }
	            }

	            break;
	        }
	    }

	    this.walker.draw();
	},

	this.isDone = function () {
	    return (walker.x == walker.maze.end.x && walker.y == walker.maze.end.y);
	},

	this.solve = function () {
	    // Draw solution path.
	    for (var x = 0; x < this.walker.maze.width; x++) {
	        for (var y = 0; y < this.walker.maze.height; y++) {
	            if (this.walker.visited[x][y] == 1) {
	                this.walker.context.fillStyle = 'red';
	                this.walker.context.fillRect(x * 10, y * 10, 10, 10);
	            }
	        }
	    }
	}
};

function mazeManager(context, maze) {
    this.context = context,
	this.width = maze.width,
	this.height = maze.height,
	this.start = maze.start,
	this.end = maze.end,
	this.maze = maze.map,

	this.draw = function (drawClear) {
	    for (var y = 0; y < this.height; y++) {
	        for (var x = 0; x < this.width; x++) {
	            if (this.isWall(x, y)) {
	                this.context.fillStyle = 'black';
	                this.context.fillRect(x * 10, y * 10, 10, 10);
	            }
	            else if (drawClear) {
	                this.context.fillStyle = 'white';
	                this.context.fillRect(x * 10, y * 10, 10, 10);
	            }
	        }
	    }
	},

	this.isWall = function (x, y) {
	    return (x < 0 || y < 0 || this.maze[x + (y * this.width)] == '*');
	},

	this.isOpen = function (x, y) {
	    return !this.isWall(x, y);
	}
};