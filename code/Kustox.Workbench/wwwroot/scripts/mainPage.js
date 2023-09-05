function startScript(commandApiUrl) {
    // Get the text area element
    const textArea = document.getElementById('scriptText');

    initResultTable();
    textArea.value = readCookie();
    setupCookieUpdate(textArea);
    setupApiHook(commandApiUrl, textArea);
}

function initResultTable() {
    $(document).ready(function () {
        $('#resultTable').DataTable({
            "scrollCollapse": true,
            "paging": false,
            "searching": false,
            "info": false,
            "scrollY": "400px"
        });
    });
}

function readCookie() {
    var memory = {};

    if (document.cookie.trim().length > 0) {
        var parts = document.cookie.split(';');

        for (var i = 0; i !== parts.length; ++i) {
            var subParts = parts[i].split('=');

            memory[subParts[0].trim()] = decodeURIComponent(subParts[1].trim());
        }
    }
    if (memory.hasOwnProperty('query') && typeof memory.query === "string") {
        return memory.query;
    }
    else {
        return ".show version";
    }
}

function setupCookieUpdate(textArea) {
    textArea.addEventListener("input", function (event) {
        document.cookie = "query=" + encodeURIComponent(event.target.value);
    });
}

function setupApiHook(commandApiUrl, textArea) {
    // Add event listener for keydown event
    textArea.addEventListener('keydown', function (event) {
        // Check if SHIFT key and ENTER key are pressed simultaneously
        if (event.shiftKey && event.key === 'Enter') {
            // Prevent the default behavior of the ENTER key within the text area
            event.preventDefault();

            // Perform your API call here using the current URL as the endpoint
            fetch(commandApiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json' // Set the appropriate headers
                },
                body: JSON.stringify({
                    Csl: textArea.value // Pass any required data to the API
                })
            })
                .then(response => response.json())
                .then(data => {
                    // Handle the API response data
                    console.log(data);
                })
                .catch(error => {
                    // Handle any API call errors
                    console.error(error);
                });
        }
    });
}