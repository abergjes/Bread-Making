window.breadAudio = (() => {
    let ctx = null;

    function getCtx() {
        if (!ctx) ctx = new AudioContext();
        if (ctx.state === 'suspended') ctx.resume();
        return ctx;
    }

    function beep(freq, duration, gain = 0.28) {
        const c = getCtx();
        const osc = c.createOscillator();
        const g   = c.createGain();
        osc.connect(g);
        g.connect(c.destination);
        osc.frequency.value = freq;
        osc.type = 'sine';
        g.gain.setValueAtTime(gain, c.currentTime);
        g.gain.exponentialRampToValueAtTime(0.001, c.currentTime + duration);
        osc.start(c.currentTime);
        osc.stop(c.currentTime + duration);
    }

    return {
        warnOverPlanned() { beep(523, 0.45); },
        warnOverMax()     { beep(880, 0.3); setTimeout(() => beep(880, 0.3), 380); }
    };
})();
