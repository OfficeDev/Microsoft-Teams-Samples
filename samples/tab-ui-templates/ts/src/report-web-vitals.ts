import type { ReportHandler } from "web-vitals";

const reportWebVitals = async (callback?: ReportHandler) => {
  if (typeof callback === "function") {
    const vitals = await import("web-vitals");
    vitals.getCLS(callback);
    vitals.getFID(callback);
    vitals.getFCP(callback);
    vitals.getLCP(callback);
    vitals.getTTFB(callback);
  }
};

export default reportWebVitals;
