export const SheetJSFT = [
	//"xlsx", "xlsb", "xlsm", "xls", "xml", "csv", "txt", "ods", "fods", "uos", "sylk", "dif", "dbf", "prn", "qpw", "123", "wb*", "wq*", "html", "htm"
	"xlsx", "xlsb", "xlsm", "xls", "xml", "csv"
].map(function(x) { return "." + x; }).join(",");