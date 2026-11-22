import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    provideRouter(routes, withComponentInputBinding()),
    providePrimeNG({
      theme: {
        preset: Aura,
      },
      license:
        'eyJpZCI6ImJjNGQyMTA5LTljNWItNGRhMS1hNmI5LTJhOGEyMmY5MGY5MCIsInByb2R1Y3QiOiJwcmltZXVpIiwidGllciI6ImNvbW11bml0eSIsInR5cGUiOiJkZXYiLCJpYXQiOjE3OTUzNTQxODcsImV4cCI6MTgyNjg5MDE4N30.yGjNv6sEDSNtpIerqHCMRnjf1dXEcl8lV-AXgd_7f2TG1eW_x57tG_M9M2KSO-wxty8mwsnycvnoSFvwFbQQBg',
    }),
  ],
};
