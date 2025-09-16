FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json
COPY Fin-Frontend/package*.json ./

# Install dependencies
RUN npm ci

# Copy the rest of the frontend code
COPY Fin-Frontend/ ./

# Build the frontend
RUN npm run build

# Production stage
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy the built assets from the build stage
COPY --from=build /app/dist .

# Copy custom nginx config
COPY Fin-Frontend/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Run nginx
CMD ["nginx", "-g", "daemon off;"]