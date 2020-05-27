import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap, switchMap } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';
import { NominationsService } from '../services';

@Component({
    selector: 'app-nominations-view',
    templateUrl: './nominations-view.component.html',
    styleUrls: ['./nominations-view.component.scss']
})
export class NominationsViewComponent implements OnInit {

    pagedNominations: Observable<any>;
    itemsPerPage = 5;
    currentPage = 1;

    constructor(
        private nominationsService: NominationsService,
        private route: ActivatedRoute,
        private router: Router) {
    }

    ngOnInit() {
        this.pagedNominations = this.route.queryParams
            .pipe(
                map(({ page }) => ({
                    page: Number(page || 1)
                })),
                tap(({ page }) => {
                    this.currentPage = page;
                }),
                switchMap(({ page }) => {
                    return this.nominationsService.searchNominations({ page });
                }));
    }

    pageChanged(event: any): void {
        const { page } = event;
        this.router.navigate(['nominations'], { queryParams: { page } });
    }

    editDataset(dataset) {
        const { id } = dataset;
        this.router.navigate(['nominations', id]);
    }
}
