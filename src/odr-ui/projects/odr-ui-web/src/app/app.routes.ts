import { ModuleWithProviders } from "@angular/core";
import { RouterModule } from "@angular/router";

import { AppLoginComponent } from "./components/app/app-login.component";
import { SearchComponent } from "./components/search/search.component";
import { AboutComponent } from "./components/about/about.component";
import { DatasetLoaderComponent } from "./components/dataset-detail/dataset-loader.component";
import { DatasetEditComponent } from "./components/dataset-edit/dataset-edit.component";
import { FaqComponent } from "./components/faq/faq.component";
import { NominateComponent } from "./components/nominate/nominate.component";
import { IssueComponent } from "./components/issue/issue.component";
import { PromptLoginForUnauthGuard } from "./shared/guards/prompt-login-for-unauth.guard";
import { GeneralIssueComponent } from "./components/issue/general-issue.component";
import { HomeComponent } from "./components/home/home.component";
import { CategoriesComponent } from "./components/categories/categories.component";

export const routeImport: ModuleWithProviders = RouterModule.forRoot([
  {
    path: "",
    component: HomeComponent
  },
  {
    path: "categories",
    component: CategoriesComponent
  },
  {
    path: "auth",
    component: AppLoginComponent
  },
  {
    path: "datasets",
    component: SearchComponent
  },
  {
    path: "datasets/:id",
    component: DatasetLoaderComponent
  },
  {
    path: "datasets/:id/edit",
    component: DatasetEditComponent
  },
  {
    path: "faq",
    component: FaqComponent
  },
  {
    path: "issue/:id",
    component: IssueComponent,
    canActivate: [PromptLoginForUnauthGuard]
  },
  {
    path: "nominate",
    component: NominateComponent,
    canActivate: [PromptLoginForUnauthGuard]
  },
  {
    path: "feedback",
    component: GeneralIssueComponent,
    canActivate: [PromptLoginForUnauthGuard]
  },
  {
    path: "about",
    component: AboutComponent
  },
  {
    path: "**",
    redirectTo: "datasets"
  }
]);
