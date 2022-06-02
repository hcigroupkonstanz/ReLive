FROM node:14

WORKDIR /srv/relive-server/

COPY package.json ./
COPY . .

RUN npm install
RUN npm run build

# REST API
EXPOSE 55211
# Colibri - Unity
EXPOSE 55212
# Colibri - Voice
EXPOSE 55213
# Colibri - Web
EXPOSE 55214

CMD [ "node", "dist/server.js" ]
