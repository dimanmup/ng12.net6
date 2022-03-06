import { Component } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { ErrorComponent } from './error/error.component';
import { Subscription } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Title } from '@angular/platform-browser';
import { Router, NavigationStart, Event as NavigationEvent } from '@angular/router';

const pageNames: { [id: string]: string; } = {
  '/': 'главная',
  '/audit': 'события',
  '/auditOfUploadings': 'события: загрузка файлов',
  '/upload': 'загрузка'
};

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {

  sub: Subscription | undefined;
  pageName: string = pageNames['/'];
  datetimeLocalString: string = new Date().toLocaleDateString();
  displayName: string | undefined;

  constructor(http: HttpClient, snackBar: MatSnackBar, private titleService: Title, private router: Router) { 
    
    this.titleService.setTitle('Главная');

    this.router.events
      .subscribe((event: NavigationEvent) => {
        if (event instanceof NavigationStart) {
          this.pageName = pageNames[event.url.split('?')[0]];
          this.datetimeLocalString = new Date().toLocaleDateString();
        }
      });

    // info: https://localhost:5011/api/info?group=docker-users
    // 500:  https://localhost:5011/api/division-by?devider=0
    this.sub = http.get(environment.uriRoot + 'api/info', {
        responseType: 'json',
        withCredentials: true
      })
      .subscribe((info: any) => {
        
        console.log(info);
        const infoDict: { [name: string]: { [name: string]: string } } = info;
        this.displayName = infoDict['domain info']['display name'] || infoDict['domain info']['LDAP response error'];
        
      }, (errorResponse: HttpErrorResponse) => {
        const config : MatSnackBarConfig = {
          verticalPosition: 'top',
          panelClass: [ 'snackbar', 'error' ],
          data: errorResponse
        };
        snackBar.openFromComponent(ErrorComponent, config);
      }, () => {
        this.sub?.unsubscribe();
        this.sub = undefined;
      });
  }

}
