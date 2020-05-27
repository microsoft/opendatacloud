
export interface CurrentUser {
  isAuthenticated: boolean;
  displayId?: string;
  name?: string;
  bearerToken?: string;
}
