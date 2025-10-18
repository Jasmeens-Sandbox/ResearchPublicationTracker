const loader = (() => {
    const $loader = $('.loader, .spinner');
    return {
        show: () => $loader.show(),
        hide: () => $loader.hide()
    };
})();


const miniLoader = {
    show(parent) {
        // Ensure parent has position: relative for proper centering
        if (getComputedStyle(parent).position === 'static') {
            parent.style.position = 'relative';
        }

        // Prevent duplicate loader
        if (parent.querySelector('.loader-overlay')) return;

        // Create overlay and spinner
        const overlay = document.createElement('div');
        overlay.className = 'loader-overlay';
        overlay.innerHTML = `<div class="loader-spinner"></div>`;

        parent.appendChild(overlay);
    },

    hide(parent) {
        const overlay = parent.querySelector('.loader-overlay');
        if (overlay) overlay.remove();
    }
};

let tables = {};
const charts = {}; // dynamic chart store keyed by canvasId

$.fn.DataTable.ext.pager.numbers_length = 6;

// Reload & cleanup when input cleared
$('#search-input').on('input propertychange paste change', () => {
    const val = $('#search-input').val().trim();
    if (val === '') {
        destroyTables();
        destroyCharts();
    }
});

document.getElementById("SearchTerm").addEventListener("click", async () => {
    const searchTerm = $('#search-input').val().trim();
    if (!searchTerm) {
        if (dataTable) dataTable.ajax.reload();
        destroyCharts();
        return;
    }

    loader.show();
    try {

        const [pubmedCounts, pubmedPubTypes, scopusCounts, scopusPubTypes] = await Promise.all([
            fetchJson(`Home/GetV1?search=${encodeURIComponent(searchTerm)}`),
            fetchJson(`Home/GetV2?search=${encodeURIComponent(searchTerm)}`),
            fetchJson(`Home/GetScopurPublicatoinsCountOverTime?search=${encodeURIComponent(searchTerm)}`),
            fetchJson(`Home/GetScopusPublicationTypeDistribution?search=${encodeURIComponent(searchTerm)}`)
        ]);

        miniLoader.show(document.getElementById('c-1'));
        createBarChart('publicationsByYear', {
            title: `Publications "${searchTerm}" By Year (Past 5 Years)`,
            labels: Object.keys(pubmedCounts),
            data: Object.values(pubmedCounts),
            showLegend: false,
            axisTitles: { x: 'Years', y: 'Number of Publications' },
            onDone: () => miniLoader.hide(document.getElementById('c-1')),
        });

        miniLoader.show(document.getElementById('c-2'));
        createBarChart('publicationsByType', {
            title: `Publication Types for "${searchTerm}" (Past 5 Years)`,
            labels: Object.keys(pubmedPubTypes),
            data: Object.values(pubmedPubTypes),
            axisTitles: { x: 'Number of Publications', y: 'Type of Publications' },
            horizontal: true,
            showLegend: false,
            colors: [
                '#4e79a7', '#f28e2b', '#e15759', '#76b7b2',
                '#59a14f', '#edc948', '#b07aa1', '#ff9da7',
                '#9c755f', '#bab0ab'
            ],
            onDone: () => miniLoader.hide(document.getElementById('c-2')),
        });

        miniLoader.show(document.getElementById('c-3'));
        createBarChart('publicationsByYearScopus', {
            title: `Publications "${searchTerm}" By Year (Past 5 Years)`,
            labels: Object.keys(scopusCounts),
            data: Object.values(scopusCounts),
            showLegend: false,
            axisTitles: { x: 'Years', y: 'Number of Publications' },
            onDone: () => miniLoader.hide(document.getElementById('c-3')),
        });

        miniLoader.show(document.getElementById('c-4'));
        createBarChart('publicationsByTypeScopus', {
            title: `Publication Types for "${searchTerm}" (Past 5 Years)`,
            labels: Object.keys(scopusPubTypes),
            data: Object.values(scopusPubTypes),
            axisTitles: { x: 'Number of Publications', y: 'Type of Publications' },
            horizontal: true,
            showLegend: false,
            colors: [
                '#4e79a7', '#f28e2b', '#e15759', '#76b7b2',
                '#59a14f', '#edc948', '#b07aa1', '#ff9da7',
                '#9c755f', '#bab0ab'
            ],
            onDone: () => miniLoader.hide(document.getElementById('c-4')),
        });


        await initOrReloadTable("pubmed-table", "Home/SearchPubmed");
        await initOrReloadTable("scopus-table", "Home/SearchScopus", true);

        Object.keys(tables).forEach(id => {
            if (tables[id]) {
                new $.fn.dataTable.Buttons(tables[id], {
                    buttons: [
                        {
                            extend: 'csvHtml5',
                            text: '💾 Save to CSV',
                            className: 'btn btn-primary btn-sm',
                            filename: `${$('#search-input').val().trim()}_${new Date().toLocaleDateString()}`
                        }
                    ]
                }).container().appendTo($(`#csv-${id}`));
            }
        });

    } catch (err) {
        console.error('Search or chart error:', err);
    } finally {
        loader.hide();
    }
});

// ---------------------- DataTable ----------------------
async function initOrReloadTable(tableId, url, isScopus = false) {
    const tableElement = document.getElementById(tableId);
    if (!tableElement) return;

    // Destroy existing table if exists
    if (tables[tableId]) {
        tables[tableId].destroy();
        tables[tableId] = null;
    }

    // Initialize DataTable
    tables[tableId] = $(tableElement).DataTable({
        processing: true,
        ordering: false,
        orderMulti: false,
        serverSide: true,
        searching: true,
        lengthMenu: [10, 25],
        pagingType: 'simple_numbers',
        language: {
            info: "Displaying _START_–_END_ of _TOTAL_ records",
            infoEmpty: "No records available"
        },
        ajax: {
            url: url,
            type: "POST",
            data: d => { d.SearchTerm = $('#search-input').val().trim(); }
        },
        columns: [
            { data: "provider", name: "Provider" },
            {
                data: "title",
                name: "Title",
                width: "250px",
                render: (data, type, row) => `<a href="${row.recordUrl}" target="_blank">${data}</a>`
            },
            {
                data: null,
                name: "Abstract",
                width: "500px",
                render: (data, type, row, meta) => {
                    if (!isScopus) {
                        // PubMed or already has abstract
                        return renderAbstract(row.abstract, type, row, meta);
                    } else {
                        // Scopus: show "Get Abstract" button

                        if (row.abstract == "No abstract available") {
                            return `<button class="btn btn-sm btn-primary get-abstract-btn" data-id="${row.providerId}" data-row="${meta.row}">
                                    Get Abstract
                                </button>
                                <div class="scopus-abstract" id="abstract-${meta.row}" style="margin-top: 5px;"></div>`;
                        }
                        else {
                            return renderAbstract(row.abstract, type, row, meta);
                        }
                    }
                }
            },
            { data: "authors", name: "Authors", width: "300px", render: renderAuthors },
            { data: "publicationDate", name: "Publication Date" }
        ]
    })
        .on('error.dt', (e, settings, techNote, message) => console.error('DataTables error:', message))
        .on('processing.dt', (e, settings, processing) => processing ? loader.show() : loader.hide());

    return tables[tableId];
}

$(document).on('click', '.get-abstract-btn', async function () {
    const button = this;
    const scopusId = button.dataset.id;
    const rowIndex = button.dataset.row;
    const abstractDiv = document.getElementById(`abstract-${rowIndex}`);

    if (button.disabled) return;
    button.disabled = true;
    button.textContent = "Loading...";

    try {
        const response = await fetchJson(`Home/GetScopusAbstract?scopusId=${scopusId}`);
        const abstractText = response.abstract || 'No abstract available';
        abstractDiv.innerHTML = renderAbstract(abstractText, null, null, { row: rowIndex });
        button.remove();
    } catch (err) {
        console.error('Failed to fetch abstract:', err);
        button.textContent = "Retry";
        button.disabled = false;
    }
});


// ---------------------- Render Helpers ----------------------
function renderAbstract(data, type, row, meta) {
    if (!data) return '';
    const maxLength = 100;
    if (data.length <= maxLength) return data;

    var unique = generateRandomString(15);
    const rowId = `collapse-abstract-${meta.row}-${unique}`;
    const shortId = `short-abstract-${meta.row}-${unique}`;
    const shortText = data.substring(0, maxLength) + '...';
    return `
        <div>
            <span id="${shortId}">${shortText}</span>
            <div class="collapse" id="${rowId}">${data}</div>
            <a class="text-primary" role="button"
               onclick="toggleAbstractInline(this, '${rowId}', '${shortId}')">
               Read more
            </a>
        </div>`;
}

function generateRandomString(length) {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    const charactersLength = characters.length;
    for (let i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}

function renderAuthors(data) {
    if (!Array.isArray(data)) return '';
    return data
        .sort((a, b) => a.authorOrder - b.authorOrder)
        .map(a => `<span class="badge text-bg-secondary">${a.name || a}</span>`)
        .join(" ");
}

// ---------------------- Chart Helpers ----------------------
async function fetchJson(url) {
    const res = await fetch(url);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
}

function destroyCharts() {
    Object.keys(charts).forEach(id => {
        if (charts[id]) {
            charts[id].destroy();
            charts[id] = null;
        }
    });
}

function destroyTables() {
    Object.keys(tables).forEach(id => {
        destroyCsvButtons(id);
        if (tables[id]) tables[id].ajax.reload();
    });
}

function destroyCsvButtons(id) {
    $(`#csv-${id}`).empty(); // remove from DOM
    if (tables[id] && tables[id].buttons) tables[id].buttons().destroy();
}

function createBarChart(canvasId, {
    title,
    labels,
    data,
    axisTitles = {},
    colors = ['rgba(54, 162, 235, 0.5)'],
    horizontal = false,
    showLegend = true,
    onDone = null,
    onStart = null,
}) {
    // Destroy existing chart on this canvas
    if (charts[canvasId]) {
        charts[canvasId].destroy();
        charts[canvasId] = null;
    }

    const ctx = document.getElementById(canvasId).getContext('2d');
    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: title,
                data,
                backgroundColor: colors,
                borderColor: colors.map(c => c.replace('0.5', '1')),
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            indexAxis: horizontal ? 'y' : 'x',
            plugins: {
                legend: { display: showLegend },
                title: { display: !!title, text: title }
            },
            scales: {
                x: { beginAtZero: true, title: { display: !!axisTitles.x, text: axisTitles.x } },
                y: { beginAtZero: true, title: { display: !!axisTitles.y, text: axisTitles.y }, ticks: { precision: 0 } }
            },
            animation: {
                onComplete: () => {
                    if (typeof onDone === 'function') onDone();
                },
                onStart: () => {
                    if (typeof onStart === 'function') onStart();
                },
            }
        }
    });
}

// ---------------------- UI ----------------------
function toggleAbstractInline(link, collapseId, shortTextId) {
    const target = document.getElementById(collapseId);
    const shortTarget = document.getElementById(shortTextId);
    const collapseInstance = bootstrap.Collapse.getOrCreateInstance(target);

    // Clean up any previous event listeners to avoid stacking
    target.removeEventListener('shown.bs.collapse', target._shownHandler);
    target.removeEventListener('hidden.bs.collapse', target._hiddenHandler);

    // Define event handlers
    target._shownHandler = () => {
        shortTarget.style.display = "none";
        link.textContent = 'Read less';
    };

    target._hiddenHandler = () => {
        shortTarget.style.display = "inline";
        link.textContent = 'Read more';
    };

    target.addEventListener('shown.bs.collapse', target._shownHandler);
    target.addEventListener('hidden.bs.collapse', target._hiddenHandler);

    collapseInstance.toggle();
}
