FROM node:22-alpine AS build
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build -- --configuration production

FROM nginx:alpine AS final
COPY --from=build /app/src/Cookbook.Api/wwwroot/ /usr/share/nginx/html/
COPY docker/nginx.cookbook.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
