const fs = require('fs')

function saveConfig(participantIndex, study, isBackup) {
    var restPort = 57001;
    var unityPort = 57002;
    if (isBackup) {
        restPort += 100;
        unityPort += 100;
    }

    if (study === 'stream') {
        restPort += 4;
        unityPort += 4;
    }

    restPort += participantIndex * 10;
    unityPort += participantIndex * 10;

    var sessionId = "remote_" + participantIndex + "_" + study;
    if (isBackup) {
        sessionId += "_BACKUP";
    }

    var sessionName = "";
    if (isBackup) {
        sessionName = "Backup " + participantIndex + " " + study;
    } else {
        sessionName = "Remote " + participantIndex + " " + study;
    }


    var config = {
        "RestPort": restPort,
        "UnityPort": unityPort,
        "Protocol": "https://",
        "ServerAddress": "lab.hci.uni-konstanz.de",
        "EnableRenderstreaming": true,
        "EnableSync": true,
        "EnableLogging": true,
        "SessionId": sessionId,
        "SessionName": sessionName,
        "SessionDescription": ""
    };

    fs.writeFile(sessionId + ".json", JSON.stringify(config, null, 4), 'utf8', function(err) {
         if (err) {
            console.log("An error occured while writing JSON Object to File.");
            return console.log(err);
        }
        
        console.log("JSON file has been saved. " + sessionId); 
    })

}

for (var i = 1; i < 5; i++) {
    saveConfig(i, "arvsvr", false);
    saveConfig(i, "arvsvr", true);
    saveConfig(i, "stream", true);
    saveConfig(i, "stream", false);
}

