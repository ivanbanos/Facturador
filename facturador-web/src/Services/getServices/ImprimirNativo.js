function ImprimirNativo(content) {
    // Open a new blank window
    const printWindow = window.open('', '_blank');

    if (printWindow) {
        // Write the content into the new window
        printWindow.document.open();
        printWindow.document.write(`
            <html>
                <head>
                    <title>Print</title>
                    <style>
                        /* Add any custom styles for the printed content */
                        body {
                            font-family: Arial, sans-serif;
                            margin: 20px;
                        }
                    </style>
                </head>
                <body>
                    ${content}
                </body>
            </html>
        `);
        printWindow.document.close();

        // Trigger the print dialog
        printWindow.print();
    } else {
        console.error('Failed to open a new window for printing.');
    }
}

export default ImprimirNativo;