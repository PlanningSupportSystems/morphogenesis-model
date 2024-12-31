// JavaScript source code
// FileSaver.js

// Função para salvar um arquivo com o nome e o conteúdo fornecidos
function SaveFile(filename, content) {
    // Cria um blob com o conteúdo do arquivo
    var blob = new Blob([content], { type: 'image/jpeg' });
    // Cria um link temporário para realizar o download
    var link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    // Adiciona o link ao documento, clica nele e remove-o
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
