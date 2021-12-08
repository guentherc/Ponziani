export function scrollToBottom(el) {
    if (el) {
        el.scrollTop = el.scrollHeight;
    }
}

export function setHeight(el, factor) {
    el.style.height = String(el.offsetWidth * factor) + "px";
}

export function setCSSProperty(el, selector, prop, value) {
    var sqlist = el.querySelectorAll(selector);
    if (sqlist) {
        for (var i = 0; i < sqlist.length; ++i) {
            sqlist[i].style.setProperty(prop, value);
        }
    }
    el.style.setProperty(prop, value);
}

export function initEngine(cb) {
    console.log("initEngine");
    if (!window.engine) {
        window.engine = new Worker("./_content/PonzianiComponents/stockfish.js");
        window.engine.callbacks = [];
        window.engine.onmessage = function (e) {
            for (let cb of window.engine.callbacks) {
                cb.invokeMethodAsync('EngineMessageAsync', e.data);
            }
        };
        send("uci");
    }
    window.engine.callbacks.push(cb);
}

export function send(message) {
    console.log("send " + message);
    window.engine.postMessage(message);
}

export function analyze(fen) {
    send("stop");
    send("position fen " + fen);
    send("go");
}