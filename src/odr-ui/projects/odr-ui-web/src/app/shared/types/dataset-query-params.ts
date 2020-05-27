import { SortOrder } from './sort-order.enum';

export class DatasetQueryParams {
    page?: number;
    sort?: SortOrder;
    term?: string;
    domain?: string;
    license?: string;
    tags?: string;
    filetypes?: string;
}
