(() => {
    const canvas = document.getElementById('gameCanvas');
    const ctx = canvas.getContext('2d');

    function resizeCanvas() {
        const rect = canvas.getBoundingClientRect();
        const ratio = window.devicePixelRatio || 1;
        canvas.width = Math.floor(rect.width * ratio);
        canvas.height = Math.floor(rect.height * ratio);
        ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
    }
    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);

    const scoreEl = document.getElementById('score');
    const livesEl = document.getElementById('lives');
    const levelEl = document.getElementById('level');
    const bestEl = document.getElementById('best');
    const btnStart = document.getElementById('btnStart');
    const btnPause = document.getElementById('btnPause');
    const btnReset = document.getElementById('btnReset');

    const modal = new bootstrap.Modal(document.getElementById('gameOverModal'));

    let score = 0, lives = 3, level = 1;
    let best = parseInt(localStorage.getItem('gym_best_score') || '0');
    bestEl.textContent = best;
    let running = false, animationId = null;
    let lastTime = performance.now();
    let spawnTimer = 0, spawnInterval = 1500, difficultyTimer = 0;

    const player = { w: 140, h: 14, x: 0, y: 0, color: '#ff6b00' };
    const items = [], particles = [];

    function resetGame() {
        score = 0; lives = 3; level = 1;
        spawnInterval = 1500; difficultyTimer = 0;
        items.length = 0; particles.length = 0;
        updateUI();
        resizeCanvas();
        const cw = canvas.width / (devicePixelRatio || 1);
        const ch = canvas.height / (devicePixelRatio || 1);
        player.x = cw / 2 - player.w / 2;
        player.y = ch - player.h - 14;
    }
    resetGame();

    function drawPlayer() {
        ctx.fillStyle = player.color;
        roundRect(ctx, player.x, player.y, player.w, player.h, 6);
        ctx.fill();
    }

    function drawItem(it) {
        ctx.save();
        ctx.translate(it.x, it.y);

        if (it.type === 'dumbbell') {
            // Стильна гантеля (срібна)
            const grad = ctx.createLinearGradient(-it.size, 0, it.size, 0);
            grad.addColorStop(0, '#999');
            grad.addColorStop(0.5, '#ddd');
            grad.addColorStop(1, '#999');
            ctx.fillStyle = grad;

            // дві сторони
            ctx.beginPath();
            ctx.arc(-it.size * 0.7, 0, it.size * 0.6, 0, Math.PI * 2);
            ctx.arc(it.size * 0.7, 0, it.size * 0.6, 0, Math.PI * 2);
            ctx.fill();

            // стрижень
            ctx.fillStyle = '#666';
            ctx.fillRect(-it.size * 0.7, -it.size * 0.15, it.size * 1.4, it.size * 0.3);

        } else if (it.type === 'heart') {
            ctx.fillStyle = '#ff3b3b';
            ctx.beginPath();
            ctx.moveTo(0, it.size / 2);
            ctx.bezierCurveTo(-it.size, -it.size / 2, -it.size / 2, -it.size, 0, -it.size / 4);
            ctx.bezierCurveTo(it.size / 2, -it.size, it.size, -it.size / 2, 0, it.size / 2);
            ctx.fill();
        } else if (it.type === 'bomb') {
            ctx.fillStyle = '#222';
            ctx.beginPath();
            ctx.arc(0, 0, it.size * 0.6, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = '#ffae00';
            ctx.fillRect(-2, -it.size * 0.9, 4, it.size * 0.3);
        }
        ctx.restore();
    }

    function drawParticles() {
        for (const p of particles) {
            ctx.fillStyle = `rgba(${p.color},${p.alpha})`;
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
            ctx.fill();
        }
    }

    function spawnParticles(x, y, colorRGB) {
        for (let i = 0; i < 15; i++) {
            particles.push({
                x, y,
                vx: (Math.random() - 0.5) * 3,
                vy: (Math.random() - 1.5) * 3,
                size: Math.random() * 3 + 2,
                color: colorRGB,
                alpha: 1.0,
                decay: 0.02 + Math.random() * 0.03
            });
        }
    }

    function updateParticles(dt) {
        for (let i = particles.length - 1; i >= 0; i--) {
            const p = particles[i];
            p.x += p.vx * (dt / 16.67);
            p.y += p.vy * (dt / 16.67);
            p.vy += 0.04;
            p.alpha -= p.decay;
            if (p.alpha <= 0) particles.splice(i, 1);
        }
    }

    function collides(it) {
        return it.x > player.x && it.x < player.x + player.w && it.y + it.size * 0.6 >= player.y;
    }

    function loop(now) {
        if (!running) return;
        const dt = now - lastTime;
        lastTime = now;
        spawnTimer += dt;
        difficultyTimer += dt;

        if (difficultyTimer >= 5000) {
            difficultyTimer = 0;
            if (spawnInterval > 700) spawnInterval -= 80;
            level++;
        }

        if (spawnTimer >= spawnInterval) {
            spawnTimer = 0;
            spawnItem();
        }

        updateItems(dt);
        updateParticles(dt);
        render();
        animationId = requestAnimationFrame(loop);
    }

    function updateItems(dt) {
        const ch = canvas.height / (devicePixelRatio || 1);
        for (let i = items.length - 1; i >= 0; i--) {
            const it = items[i];
            it.vy += it.gravity * (dt / 16.67);
            it.y += it.vy * (dt / 16.67);

            if (collides(it)) {
                if (it.type === 'dumbbell') {
                    score++;
                    spawnParticles(it.x, it.y, '255,255,255');
                } else if (it.type === 'heart') {
                    lives++;
                    spawnParticles(it.x, it.y, '255,60,60');
                } else if (it.type === 'bomb') {
                    lives--;
                    score -= 2;
                    spawnParticles(it.x, it.y, '255,120,0');
                }
                items.splice(i, 1);
                updateUI();
                if (lives <= 0) return gameOver();
                continue;
            }

            if (it.y - it.size > ch) {
                items.splice(i, 1);
                if (it.type === 'dumbbell') {
                    lives--;
                    if (lives <= 0) return gameOver();
                    updateUI();
                }
            }
        }
    }

    function spawnItem() {
        const cw = canvas.width / (devicePixelRatio || 1);
        const types = ['dumbbell', 'dumbbell', 'dumbbell', 'heart', 'bomb'];
        const type = types[Math.floor(Math.random() * types.length)];
        const size = 18 + Math.random() * 16;
        const baseSpeed = 0.6 + Math.random() * 0.8 + level * 0.05;
        const gravity = 0.15 + Math.random() * 0.1 + level * 0.02;

        items.push({
            x: Math.random() * (cw - 40) + 20,
            y: -20,
            vy: baseSpeed,
            gravity,
            size,
            type
        });
    }

    function render() {
        const cw = canvas.width / (devicePixelRatio || 1);
        const ch = canvas.height / (devicePixelRatio || 1);
        ctx.clearRect(0, 0, cw, ch);

        const g = ctx.createLinearGradient(0, 0, 0, ch);
        g.addColorStop(0, '#0b0b0b');
        g.addColorStop(1, '#141414');
        ctx.fillStyle = g;
        ctx.fillRect(0, 0, cw, ch);

        for (const it of items) drawItem(it);
        drawPlayer();
        drawParticles();
    }

    function updateUI() {
        scoreEl.textContent = score;
        livesEl.textContent = lives;
        levelEl.textContent = level;
        bestEl.textContent = best;
    }

    function gameOver() {
        running = false;
        cancelAnimationFrame(animationId);
        if (score > best) {
            best = score;
            localStorage.setItem('gym_best_score', best);
        }
        document.getElementById('finalScore').textContent = score;
        document.getElementById('bestScore').textContent = best;
        modal.show();
    }

    btnStart.addEventListener('click', () => {
        if (!running) {
            running = true;
            lastTime = performance.now();
            animationId = requestAnimationFrame(loop);
        }
    });
    btnPause.addEventListener('click', () => {
        if (running) {
            running = false;
            cancelAnimationFrame(animationId);
        } else {
            running = true;
            lastTime = performance.now();
            animationId = requestAnimationFrame(loop);
        }
    });
    btnReset.addEventListener('click', () => {
        running = false;
        cancelAnimationFrame(animationId);
        resetGame();
    });

    canvas.addEventListener('mousemove', (e) => {
        const rect = canvas.getBoundingClientRect();
        player.x = e.clientX - rect.left - player.w / 2;
        clampPlayer();
    });
    canvas.addEventListener('touchmove', (e) => {
        const rect = canvas.getBoundingClientRect();
        player.x = e.touches[0].clientX - rect.left - player.w / 2;
        clampPlayer();
    }, { passive: true });

    function clampPlayer() {
        const cw = canvas.width / (devicePixelRatio || 1);
        if (player.x < 0) player.x = 0;
        if (player.x + player.w > cw) player.x = cw - player.w;
    }

    function roundRect(ctx, x, y, w, h, r) {
        if (w < 2 * r) r = w / 2;
        if (h < 2 * r) r = h / 2;
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.arcTo(x + w, y, x + w, y + h, r);
        ctx.arcTo(x + w, y + h, x, y + h, r);
        ctx.arcTo(x, y + h, x, y, r);
        ctx.arcTo(x, y, x + w, y, r);
        ctx.closePath();
    }
})();
