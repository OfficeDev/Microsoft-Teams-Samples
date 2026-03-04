// Initialize Teams SDK when the script is loaded
microsoftTeams.app.initialize();

window.captureMedia = function () {
    navigator.mediaDevices.getUserMedia({ video: true })
        .then(mediaStream => {
            const track = mediaStream.getVideoTracks()[0];
            const imageCapture = new ImageCapture(track);
            return imageCapture.takePhoto();
        })
        .then(blob => {
            const imageElement = document.getElementById("capturedImage");
            imageElement.src = URL.createObjectURL(blob);
            imageElement.style.display = "block";
        })
        .catch(error => console.error("Error capturing image:", error));
};

async function captureAudio() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const audioElement = document.getElementById("audioPlayer");
        
        if (!audioElement) {
            console.error("Audio player element not found!");
            alert("Error: No audio player element found on the page.");
            return;
        }

        audioElement.srcObject = stream;
        audioElement.play();
    } catch (error) {
        console.error("Error capturing audio:", error);
        alert("Error capturing audio: " + error.message);
    }
}

function captureVideo() {
    navigator.mediaDevices.getUserMedia({ video: true })
        .then(mediaStream => {
            const videoElement = document.getElementById("videoElement");
            videoElement.srcObject = mediaStream;
        })
        .catch(error => console.error("Error accessing camera:", error));
}

function getCurrentLocation() {
    navigator.permissions.query({ name: 'geolocation' }).then((result) => {
        if (result.state === 'denied') {
            document.getElementById("permissionsWarning").classList.remove("hidden");
            document.getElementById("getLocationBtn").disabled = true;
        } else {
            document.getElementById("permissionsWarning").classList.add("hidden");
            document.getElementById("getLocationBtn").disabled = false;
        }
    });

    navigator.geolocation.getCurrentPosition(
        (position) => {
            const location = {
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            };
            document.getElementById("locationText").innerText = 
                `Latitude: ${location.latitude}, Longitude: ${location.longitude}`;
        },
        (error) => {
            console.error("Error fetching location:", error);
            alert("Error fetching location: " + error.message);
        }
    );
}

function sendNotification() {
    navigator.permissions.query({ name: 'notifications' }).then((result) => {
        if (result.state === 'denied') {
            alert("Notification permission denied.");
        } else {
            console.log("Permission result:", result);
            alert("Notification permission granted.");
        }

        if (Notification.permission !== 'granted') {
            Notification.requestPermission().then(permission => {
                if (permission === "granted") {
                    createNotification();
                } else {
                    alert("Notifications are blocked.");
                }
            });
        } else {
            createNotification();
        }
    });
}

function createNotification() {
    var notification = new Notification('Sample Notification', {
        body: 'Hey there! You\'ve been notified!',
    });
    notification.onclick = function () {
        window.open('https://github.com/OfficeDev/Microsoft-Teams-Samples');
    };
}


/* Teams client functions */

function captureMultipleImages(mediaCount) {
    let imageProps = {
        sources: [microsoftTeams.media.Source.Camera, microsoftTeams.media.Source.Gallery],
        startMode: microsoftTeams.media.CameraStartMode.Photo,
        ink: false,
        cameraSwitcher: false,
        textSticker: false,
        enableFilter: true,
    };

    let mediaInput = {
        mediaType: microsoftTeams.media.MediaType.Image,
        maxMediaCount: mediaCount,
        imageProps: imageProps
    };

    microsoftTeams.media.selectMedia(mediaInput, (error, attachments) => {
        if (error) {
            alert("Error: " + error.errorCode + " - " + (error.message || ""));
        } else if (attachments) {
            const imageContainer = document.getElementById("imageContainer");
            imageContainer.innerHTML = "";
            attachments.forEach(item => {
                let imgElement = document.createElement("img");
                imgElement.src = "data:" + item.mimeType + ";base64," + item.preview;
                imgElement.style.width = "100px";
                imgElement.style.margin = "10px";
                imageContainer.appendChild(imgElement);
            });
        }
    });
}

function selectPeople() {
    microsoftTeams.people.selectPeople().then(people => {
        let peopleDiv = document.getElementById("selectedPeople");
        if (people.length === 0) {
            peopleDiv.innerHTML = "<p>No people selected.</p>";
            return;
        }

        let peopleList = people.map(p => `<p>${p.displayName}</p>`).join('');
        peopleDiv.innerHTML = `<p>Selected ${people.length} people:</p> ${peopleList}`;
    }).catch(error => {
        alert("Error: " + error.errorCode + " " + (error.message || ""));
    });
}