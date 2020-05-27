/**
 * User status
 */
export enum UserStatus {

    /**
     * Anonymous user aka unknown
     */
    Anonymous = 0,

    /**
     * User has been authenticated, has not accepted current dataset license
     */
    Authenticated = 1,

    /**
     * User is authenticated and has accepted current dataset license
     */
    AuthenticatedLicenseAccepted = 2,

    /**
     * Set user status to undefined
     */
    Undefined = 3,
}
