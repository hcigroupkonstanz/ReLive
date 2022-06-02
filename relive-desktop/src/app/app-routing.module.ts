import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { NotebookComponent } from './components/notebook/notebook.component';
import { StudyListComponent } from './components/study-list/study-list.component';
import { ReliveResolver } from './ReliveResolver';

const routes: Routes = [
    {
            path: 'studies',
            component: StudyListComponent
    },
    {
            path: 'studies/:studyId',
            component: NotebookComponent,
            resolve: {
                data: ReliveResolver
            }
    },
    {
            path: '',
            redirectTo: '/studies',
            pathMatch: 'full'
    }
];


@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }
