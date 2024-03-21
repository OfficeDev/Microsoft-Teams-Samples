import { PreventIframe } from "express-msteams-host";

/**
 * Used as place holder for the decorators
 */
@PreventIframe("/deepLinksTab/index.html")
@PreventIframe("/deepLinksTab/config.html")
@PreventIframe("/deepLinksTab/remove.html")
export class DeepLinksTab {
}
