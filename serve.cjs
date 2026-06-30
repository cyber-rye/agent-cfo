const http = require('http');
const { createProxyServer } = require('http-proxy');
const { readFile } = require('fs/promises');
const { join, extname } = require('path');

const DIST = join(__dirname, 'dashboard/dist');
const API = 'http://localhost:5077';

const MIME = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.svg': 'image/svg+xml',
  '.json': 'application/json',
  '.png': 'image/png',
  '.ico': 'image/x-icon',
};

const proxy = createProxyServer({ target: API });

proxy.on('error', (err, req, res) => {
  res.writeHead(502, { 'Content-Type': 'text/plain' });
  res.end('API unavailable');
});

const server = http.createServer(async (req, res) => {
  // Proxy /api/* to the .NET backend
  if (req.url.startsWith('/api')) {
    return proxy.web(req, res);
  }

  // Serve static files
  try {
    const filePath = join(DIST, req.url === '/' ? 'index.html' : req.url);
    const content = await readFile(filePath);
    const ext = extname(filePath);
    res.writeHead(200, { 'Content-Type': MIME[ext] || 'application/octet-stream' });
    res.end(content);
  } catch {
    // SPA fallback
    const html = await readFile(join(DIST, 'index.html'));
    res.writeHead(200, { 'Content-Type': 'text/html' });
    res.end(html);
  }
});

server.listen(3000, '0.0.0.0', () => {
  console.log('AgentCFO dashboard: http://0.0.0.0:3000');
  console.log('API proxy → ' + API);
});
