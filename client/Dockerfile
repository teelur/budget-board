FROM node:23 AS build-stage

WORKDIR /app

RUN corepack enable

COPY ["./package.json", "./yarn.lock", "./.yarnrc.yml", "./"]
RUN yarn install

COPY . .
RUN yarn run build

# production stage
FROM nginx:stable-alpine AS production-stage

COPY --from=build-stage /app/dist /usr/share/nginx/html

COPY .docker/nginx/conf/nginx-substitute.sh .
COPY .docker/nginx/conf/default.conf.template .

EXPOSE ${PORT}
ENTRYPOINT [ "/bin/sh", "nginx-substitute.sh"]