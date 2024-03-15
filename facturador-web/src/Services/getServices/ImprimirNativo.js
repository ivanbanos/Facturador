
const ImprimirNativo = async (content) => {
var pri = window.open('','', 'height=1200,width=400');


pri.document.write('<html><head>');
pri.document.write('</head>');
pri.document.write('<body ><p style={font-size: 8px;font-family: Consolas;}></p>');

const myArray = content.split("\n");
for (const element of myArray) { 
    
pri.document.write('<p style={font-size: 8px;font-family: Consolas;margin:0px;}>'+element+'</p>');
    }
pri.document.write('</body></html>');
pri.document.close(); 
pri.focus();
setTimeout(function(){pri.print(); pri.close();},1000);
//pri.print(); pri.close();
};
  
export default ImprimirNativo;
