import { Component, OnInit, OnDestroy } from '@angular/core';
import { OdrService } from '../../shared/services';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DatasetDomainType } from '../../shared/types';

@Component({
    selector: 'app-categories',
    templateUrl: './categories.component.html',
    styleUrls: ['./categories.component.scss']
})
export class CategoriesComponent implements OnInit {

    public domains: Observable<DatasetDomainType[]>;

    constructor(private odrService: OdrService) { }

    ngOnInit() {
        this.domains = this.odrService
            .getDomainsInUseByDatasets()
            .pipe(
                map((domains) => {
                    return domains.map(domain => ({
                        ...domain,
                        iconPath: this.getIconPath(domain.id),
                    }));
                })
            );
    }

    getIconPath(domainId: string): string {
        const id = domainId.toLowerCase().replace(/\s/g, '');
        return `/assets/icons/${id}.svg`;
    }
}
