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
            const currentQuery = getCurrentQuery(event.target);

            // Prevent the default behavior of the ENTER key within the text area
            event.preventDefault();

            // Perform your API call here using the current URL as the endpoint
            fetch(commandApiUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json' // Set the appropriate headers
                },
                body: JSON.stringify({
                    Csl: currentQuery // Pass any required data to the API
                })
            })
                .then(response => response.json())
                .then(data => {
                    // Handle the API response data
                    console.log(data);
                    updateResultTable(data);
                })
                .catch(error => {
                    // Handle any API call errors
                    console.error(error);
                });
        }
    });
}

function updateResultTable(data) {
    //  Destroy the dataTable
    const dataTable = $('#resultTable').DataTable();

    dataTable.destroy();

    //  Repopulate the underlying HTML table
    const resultTable = document.getElementById('resultTable');
    const thead = document.createElement('thead');
    const theadRow = document.createElement('tr');
    const tbody = document.createElement('tbody');

    //  Populate new header
    for (const column of Object.values(data.table.columns)) {
        const th = document.createElement('th');

        th.textContent = column.name;
        theadRow.appendChild(th);
    }
    thead.appendChild(theadRow);

    //  Populate new data rows
    for (const dataRow of Object.values(data.table.data)) {
        const tr = document.createElement('tr');

        for (const item of Object.values(dataRow)) {
            const td = document.createElement('td');

            td.textContent = item;
            tr.appendChild(td);
        }
        tbody.appendChild(tr);
    }
    thead.appendChild(theadRow);

    // Flip content
    resultTable.innerHTML = '';
    resultTable.appendChild(thead);
    resultTable.appendChild(tbody);

    //  Re-bind dataTables
    initResultTable();
}

function getCurrentQuery(textArea) {
    const text = textArea.value;
    const startPosition = textArea.selectionStart;
    const endPosition = textArea.selectionEnd;

    if (startPosition == endPosition) {
        // Split the text into queries based on empty lines
        const queries = text.split(/\n\s*\n/);
        // Find the query where the cursor is
        let currentPosition = 0;

        for (let i = 0; i < queries.length; i++) {
            const queryLength = queries[i].length + 1; // +1 for the newline character

            if (startPosition >= currentPosition
                && startPosition <= currentPosition + queryLength) {
                return queries[i];
            }

            currentPosition += queryLength;
        }

        return "";
    }
    else {
        return text.substring(startPosition, endPosition);
    }
}