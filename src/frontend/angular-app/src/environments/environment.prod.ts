export const environment = {
  production: true,
  gatewayUrl: '/gateway',           // Relative — assumes reverse proxy in front of the app
  catalogImageBaseUrl: ''           // Images served via gateway or CDN — set to your domain in CI/CD
};
