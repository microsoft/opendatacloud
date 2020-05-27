export interface ServerError {
  propertyName: string;
  errorCode: string;
  errorMessage: string;
}

export function isServerErrors(errors?: any): errors is ServerError[] {
  return errors && Array.isArray(errors) && errors.length > 0;
}
