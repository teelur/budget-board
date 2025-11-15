FROM node:current-alpine AS build-stage

WORKDIR /app

COPY ["./package.json", "./yarn.lock", "./.yarnrc.yml", "./"]
RUN rm -f /usr/local/bin/yarn /usr/local/bin/yarnpkg && npm install --global corepack@latest && corepack enable && yarn install

COPY . .
RUN yarn run build

# production stage
FROM nginx:alpine AS production-stage

COPY --from=build-stage /app/dist /usr/share/nginx/html

COPY .docker/nginx/conf/nginx-substitute.sh .
COPY .docker/nginx/conf/default.conf.template .

EXPOSE ${PORT}
ENTRYPOINT [ "/bin/sh", "nginx-substitute.sh"]