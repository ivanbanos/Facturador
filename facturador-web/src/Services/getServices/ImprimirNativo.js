
const ImprimirNativo = async (content) => {
var pri = window.open('','PRINT', 'height=1200,width=400');
pri.document.open();

pri.document.head.append(document.head)
pri.document.write('<html><head>');
pri.document.write('<style type="text/css" media="print">p {font-size: 8px;} html, body {height:100%; margin: 0 !important; padding: 0 !important;overflow: hidden;}</style>');
pri.document.write('</head>');
pri.document.write('<body ><p style={font-size: 8px;}>Factura prueba<br />');
pri.document.write(content.replace("\n", "<br />"));

pri.document.write('</p></body></html>');
pri.document.close(); 
pri.focus();
setTimeout(function(){pri.print();},1000);
pri.close();
};
  
export default ImprimirNativo;
