const { src, dest, series, task } = require("gulp");
const fs = require("fs"),
  path = require("path"),
  PluginError = require("plugin-error"),
  ZSchema = require("z-schema"),
  log = require("fancy-log"),
  axios = require("axios"),
  zip = require("gulp-zip"),
  argv = require("yargs").argv,
  replace = require("gulp-token-replace");

// Config
const config = require("./gulp.config");
const package = require("./package.json");

const env = argv["env"];
if (env === undefined) {
  require("dotenv").config();
} else {
  log(`Using custom .env`);
  require("dotenv").config({ path: path.resolve(process.cwd(), env) });
}
process.env.VERSION = package.version;

/**
 * Replace parameters in the manifest
 */
task("generate-manifest", (cb) => {
  return (
    src("src/manifest/manifest.json")
      .pipe(
        replace({
          tokens: {
            ...process.env,
          },
        })
      )
      .pipe(dest("./temp"))
  );
});

/**
 * Schema validation
 */
task("schema-validation", (callback) => {
  let filePath = path.join(__dirname, "temp/manifest.json");

  if (fs.existsSync(filePath)) {
    let manifest = fs.readFileSync(filePath, {
        encoding: "UTF-8",
      }),
      manifestJson;

    try {
      manifestJson = JSON.parse(manifest);
    } catch (error) {
      callback(new PluginError(error.message));
      return;
    }

    log("Using manifest schema " + manifestJson.manifestVersion);

    let definition = config.SCHEMAS.find(
      (s) => s.version == manifestJson.manifestVersion
    );

    if (definition === undefined) {
      callback(new PluginError("validate-manifest", "Unable to locate schema"));
      return;
    }

    if (manifestJson["$schema"] !== definition.schema) {
      log(
        "Note: the defined schema in your manifest does not correspond to the manifestVersion"
      );
    }

    let requiredUrl = definition.schema;
    let validator = new ZSchema();

    let schema = {
      $ref: requiredUrl,
    };

    axios
      .get(requiredUrl, {
        decompress: true,
        responseType: "json",
      })
      .then((response) => {
        validator.setRemoteReference(requiredUrl, response.data);

        var valid = validator.validate(manifestJson, schema);
        var errors = validator.getLastErrors();
        if (!valid) {
          callback(
            new PluginError(
              "validate-manifest",
              errors
                .map((e) => {
                  return e.message;
                })
                .join("\n")
            )
          );
        } else {
          callback();
        }
      })
      .catch((err) => {
        log.warn("WARNING: unable to download and validate schema: " + err);
        callback();
      });
  } else {
    console.log("Manifest doesn't exist");
  }
});

/**
 * Creates the tab manifest
 */
task("zip", () => {
  return src("./src/manifest/**/*.*")
    .pipe(src("./temp/manifest.json"))
    .pipe(zip(config.manifestFileName))
    .pipe(dest("package"));
});

task("manifest", series("generate-manifest", "schema-validation", "zip"));

task("start-ngrok", (cb) => {
  log("[NGROK] starting ngrok...");
  const ngrok = require("ngrok");

  const conf = {
    subdomain: process.env.NGROK_SUBDOMAIN,
    region: process.env.NGROK_REGION,
    addr: process.env.PORT,
    authtoken: process.env.NGROK_AUTH,
  };

  ngrok
    .connect(conf)
    .then((url) => {
      log("[NGROK] Url: " + url);
      if (!conf.authtoken) {
        log(
          "[NGROK] You have been assigned a random ngrok URL that will only be available for this session. You wil need to re-upload the Teams manifest next time you run this command."
        );
      }
      let hostName = url.replace("http://", "");
      hostName = hostName.replace("https://", "");

      log("[NGROK] HOSTNAME: " + hostName);
      process.env.HOSTNAME = hostName;

      cb();
    })
    .catch((err) => {
      log.error(`[NGROK] Error: ${JSON.stringify(err)}`);
      cb(err.msg);
    });
});
