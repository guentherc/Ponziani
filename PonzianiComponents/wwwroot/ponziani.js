export function scrollToBottom(el) {
    el.scrollTop = el.scrollHeight;
}

export function setHeight(el, factor) {
    el.style.height = String(el.offsetWidth * factor) + "px";
}