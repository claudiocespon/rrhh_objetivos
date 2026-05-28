class FluidWaves {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) return;
        this.ctx = this.canvas.getContext('2d');
        this.time = 0;
        this.resize();
        window.addEventListener('resize', () => this.resize());
        this.animate();
    }
    
    resize() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }
    
    animate() {
        requestAnimationFrame(() => this.animate());
        this.time += 0.004; // Smooth, slow movement
        const ctx = this.ctx;
        const w = this.canvas.width;
        const h = this.canvas.height;
        
        // Deep corporate dark background
        ctx.fillStyle = '#020611';
        ctx.fillRect(0, 0, w, h);
        
        // Use additive blending for a glowing, high-tech aesthetic
        ctx.globalCompositeOperation = 'lighter';
        
        // Draw layers of fluid abstract waves
        this.drawWave(ctx, w, h, 0,   '#0d1b3e', 0.6); // Deep blue
        this.drawWave(ctx, w, h, 2.5, '#162a5e', 0.4); // Mid blue
        this.drawWave(ctx, w, h, 5,   '#0a3b2b', 0.3); // Very dark green
        this.drawWave(ctx, w, h, 7.5, '#2ecc71', 0.08); // Accent green (very subtle glow)
        
        ctx.globalCompositeOperation = 'source-over';
    }
    
    drawWave(ctx, w, h, offset, color, opacity) {
        ctx.beginPath();
        // Start from left bottom
        ctx.moveTo(0, h);
        
        // Draw the wave across the width
        for (let x = 0; x <= w; x += 30) {
            // Complex multi-frequency wave calculation for a fluid, organic feel
            const freq1 = x * 0.0015;
            const freq2 = x * 0.0025;
            const freq3 = x * 0.0008;
            
            const y1 = Math.sin(freq1 + this.time + offset) * (h * 0.2);
            const y2 = Math.cos(freq2 + this.time * 0.8 + offset) * (h * 0.15);
            const y3 = Math.sin(freq3 - this.time * 0.5 + offset) * (h * 0.25);
            
            // Base line is slightly below center
            const y = (h * 0.6) + y1 + y2 + y3;
            ctx.lineTo(x, y);
        }
        
        // Close the shape to the right bottom
        ctx.lineTo(w, h);
        ctx.lineTo(0, h);
        ctx.closePath();
        
        // Fill with color
        ctx.fillStyle = color;
        ctx.globalAlpha = opacity;
        ctx.fill();
        ctx.globalAlpha = 1;
    }
}

window.initLoginAnimation = () => {
    new FluidWaves('login-canvas');
};
