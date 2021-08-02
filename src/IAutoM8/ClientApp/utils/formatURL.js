export function formatURL (text) {
    const urlRegex = /(https?:\/\/[^\s]+)/g;

    const anchorTag = text.replace(urlRegex, (url) => {
        return `<a href="${url}" target="_blank" rel="noopener noreferrer">${url}</a>`;
    });

    const url = anchorTag.split('\n').map(i => {
        return `${i}<br/>`
    }).join("");

    return url;
}