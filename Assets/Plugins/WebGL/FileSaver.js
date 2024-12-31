// JavaScript source code
// FileSaver.js

// Fun��o para salvar um arquivo com o nome e o conte�do fornecidos
function SaveFile(filename, content) {
    // Cria um blob com o conte�do do arquivo
    var blob = new Blob([content], { type: 'image/jpeg' });
    // Cria um link tempor�rio para realizar o download
    var link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    // Adiciona o link ao documento, clica nele e remove-o
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
