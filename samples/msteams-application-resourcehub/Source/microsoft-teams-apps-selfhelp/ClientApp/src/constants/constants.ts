export default class Constants {
	public static readonly maxWidthForMobileView: number = 750;
	public static readonly body: string = "body";
	public static readonly theme: string = "theme";
	public static readonly default: string = "default";
	public static readonly light: string = "light";
	public static readonly dark: string = "dark";
	public static readonly contrast: string = "contrast";
	public static readonly billableStatusColor: string = "#61AEE5";
	public static readonly nonBillableStatusColor: string = "#D54130";
	public static readonly nonUtilizedStatusColor: string = "#858C98";
	// The calendar date on which past month timesheet will get freeze.
	public static readonly timesheetFreezeDate: number = 10;
	public static readonly apiBaseUrl = window.location.origin + "/api";
	public static readonly ProjectTitleMaxLength: number = 50;
	public static readonly TaskMaxLength: number = 300;
	public static readonly reasonDescriptionMaxLength: number = 100;
	public static readonly recordPerPage: number = 10;	
	public static readonly Shared_To_User: string = "Shared to user";
	public static readonly Shared_To_Channel: string = "Shared to Channel";
}