<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0" />

    <title>Quiz Buzz</title>

    <link href="assets/css/main.css" rel="stylesheet" />

    <script src="assets/js/jquery-2.1.0.min.js"></script>
    <script src="assets/js/circle-progress.min.js"></script>
    <script src="assets/js/WebSocketCommunication.js"></script>
    <script src="assets/js/Main.js"></script>
    <script src="assets/js/howler.core.min.js"></script>

    <script>
        var WebSocketServerURL = '<%=System.Configuration.ConfigurationManager.AppSettings("WebSocketServerURL").ToString() %>';
    </script>
</head>
<body oncontextmenu="return false;">
    <div id="Main">
        <div id="Players"></div>
        
        <div id="Question">This is the question?</div>
        
        <div id="TimerOuter"></div>
        <div id="Timer"></div>
        
        <ul id="Answers">
            <li id="BLUE">Blue answer</li>
            <li id="ORANGE">Orange answer</li>
            <li id="GREEN">Green answer</li>
            <li id="YELLOW">Yellow answer</li>
        </ul>

        <div id="centered_container">
            <div id="trivia_settings">

                <label for="trivia_amount">Number of Questions:</label>
                <input type="number" name="trivia_amount" id="trivia_amount" class="form-control" min="1" max="50" value="10">

                <label for="trivia_category">Category:</label>
                <select id="trivia_category" name="trivia_category" class="form-control">
                    <option value="any">Any Category</option>
                    <option value="9">General Knowledge</option>
                    <option value="10">Entertainment: Books</option>
                    <option value="11">Entertainment: Film</option>
                    <option value="12">Entertainment: Music</option>
                    <option value="13">Entertainment: Musicals &amp; Theatres</option>
                    <option value="14">Entertainment: Television</option>
                    <option value="15">Entertainment: Video Games</option>
                    <option value="16">Entertainment: Board Games</option>
                    <option value="17">Science &amp; Nature</option>
                    <option value="18">Science: Computers</option>
                    <option value="19">Science: Mathematics</option>
                    <option value="20">Mythology</option>
                    <option value="21">Sports</option>
                    <option value="22">Geography</option>
                    <option value="23">History</option>
                    <option value="24">Politics</option>
                    <option value="25">Art</option>
                    <option value="26">Celebrities</option>
                    <option value="27">Animals</option>
                    <option value="28">Vehicles</option>
                    <option value="29">Entertainment: Comics</option>
                    <option value="30">Science: Gadgets</option>
                    <option value="31">Entertainment: Japanese Anime &amp; Manga</option>
                    <option value="32">Entertainment: Cartoon &amp; Animations</option>
                </select>

                <label for="trivia_difficulty">Difficulty:</label>
                <select id="trivia_difficulty" name="trivia_difficulty" class="form-control">
                    <option value="any">Any Difficulty</option>
                    <option value="easy">Easy</option>
                    <option value="medium">Medium</option>
                    <option value="hard">Hard</option>
                </select>
            
            </div>
            <div id="PlayerScores">
                <div id="PlayerScoresText"></div>
                <div id="PlayerScoresPlayers"></div>
            </div>
            <div id="btnOK">OK</div>
        </div>

    </div>
</body>
</html>
