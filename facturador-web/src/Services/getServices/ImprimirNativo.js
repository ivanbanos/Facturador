
const ImprimirNativo = async (content) => {
var pri = window.open('','PRINT', 'height=1200,width=400');


pri.document.write('<html><head>');
pri.document.write('<style type="text/css" media="print">p {font-size: 8px;} html, body {height:100%; margin: 0 !important; padding: 0 !important;overflow: hidden;}</style>');
pri.document.write('</head>');
pri.document.write('<body ><p style={font-size: 8px;font-family: Consolas;}></p>');

const myArray = content.split("\n");
for (const element of myArray) { // You can use `let` instead of `const` if you like
    
pri.document.write('<p style={font-size: 8px;font-family: Consolas;}>'+element+'</p>');
    }
pri.document.write('</body></html>');
pri.document.close(); 
pri.focus();
setTimeout(function(){pri.print(); pri.close();},1000);

};
  
export default ImprimirNativo;
