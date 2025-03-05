/// </summary>
///  logitem to show the activity log
/// </summary>
export function logItem(action: string, actionColor: string, message: string) {

    return ("<span style='font-weight:bold;color:" +
        actionColor +
        "'>" +
        action +
        "</span> " +
        message +
        "</br>");
}
