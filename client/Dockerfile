FROM node:current-alpine AS build-stage

WORKDIR /app

COPY ["./package.json", "./yarn.lock", "./.yarnrc.yml", "./"]

# Remove pre-installed Yarn binaries to ensure only Corepack-managed Yarn is used (avoids version conflicts)
RUN rm -f /usr/local/bin/yarn /usr/local/bin/yarnpkg
RUN npm install --global corepack@latest
RUN corepack enable
RUN yarn install

COPY . .
RUN yarn run build

# production stage
FROM nginx:alpine AS production-stage

COPY --from=build-stage /app/dist /usr/share/nginx/html

COPY .docker/nginx/conf/nginx-substitute.sh .
COPY .docker/nginx/conf/default.conf.template .

EXPOSE ${PORT}
ENTRYPOINT [ "/bin/sh", "nginx-substitute.sh"]