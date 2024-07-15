const fs = require("fs");
const path = require("path");

console.log("Ensuring env files exist...");

const envPath = path.join(__dirname, "..", "env");
const envs = [
  {
    name: ".env.local",
    content: `TEAMSFX_ENV=local`,
  },
  {
    name: ".env.local.user",
    content: `SECRET_BOT_PASSWORD=`,
  },
  {
    name: ".env.dev",
    content: `TEAMSFX_ENV=dev`,
  },
  {
    name: ".env.dev.user",
    content: `SECRET_BOT_PASSWORD=`,
  },
];

envs.forEach((env) => {
  const envFilePath = path.join(envPath, env.name);
  if (!fs.existsSync(envFilePath)) {
    fs.mkdirSync(envPath, { recursive: true });
    fs.writeFileSync(envFilePath, env.content);
  }
});

console.log("Done!");