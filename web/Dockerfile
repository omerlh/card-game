FROM node AS build
COPY . /app
WORKDIR /app
RUN yarn global add elm \
    && elm-make --yes Main.elm --output index.html

FROM nginx
WORKDIR /app
COPY --from=build /app/index.html /usr/share/nginx/html