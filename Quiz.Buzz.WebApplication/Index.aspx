﻿<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0" />

    <title>Buzz</title>

    <link href="assets/css/main.css" rel="stylesheet" />

    <script src="assets/js/jquery-2.1.0.min.js"></script>
    <script src="assets/js/circle-progress.min.js"></script>
    <script src="assets/js/WebSocketCommunication.js"></script>
    <script src="assets/js/Main.js"></script>
    <script>
        var WebSocketServerURL = '<%=System.Configuration.ConfigurationManager.AppSettings("WebSocketServerURL").ToString() %>';
    </script>
</head>
<body oncontextmenu="return false;">
    <div id="Main">       
        <div id="Players"></div>        
        <div id="Question">Is this is a very difficult question? With lots and lots of text. Very annoying indeed!<br />Bla bla bla...</div>
        <div id="TimerOuter"></div>
        <div id="Timer"></div>
        <ul id="Answers">
            <li id="BLUE">I don't know</li>
            <li id="ORANGE">42</li>
            <li id="GREEN">How should I know?</li>
            <li id="YELLOW">Are you stupid?</li>
        </ul>
        <div id="btnOK">OK</div>
    </div>
</body>
</html>