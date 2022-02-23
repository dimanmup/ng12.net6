import { HttpClient, HttpErrorResponse, HttpEventType, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ErrorComponent } from '../error/error.component';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.less']
})
export class UploadComponent implements OnInit {

  constructor(private http: HttpClient, private snackBar: MatSnackBar, private titleService: Title) { 
    this.titleService.setTitle('Загрузка файлов');
  }

  fileSizeMB: number | undefined;
  fileName: string | undefined;
  sub: Subscription | undefined;
  progress: number | undefined;
  message: string | undefined;
  messageStatus: string = '';
 
  onFileSelected(event: any): void {
    console.log(event);
    const file: File = event.target.files[0];

    if (file) {

      this.fileSizeMB = Math.round(file.size / 1e3) / 1e3;
      this.fileName = file.name;
      this.message = undefined;
      this.messageStatus = '';

      const formData = new FormData();
      formData.append('thumbnail', file);

      this.sub =this.http.post(environment.uriRoot + 'api/upload', formData, {
        withCredentials: true, // includes cookies
        reportProgress: true,
        observe: 'events'
      }).subscribe(event => {
        if (event.type == HttpEventType.UploadProgress)
          this.progress = event.total ? Math.round(100 * (event.loaded / event.total!)) : undefined;
        else if (event.type === HttpEventType.Response) {
          this.message = 'Загружено!';
          this.messageStatus = 'success';
        }
      }, (errorResponse: HttpErrorResponse) => {
        // Из-за буферизации данные попадают в контроллер на каком-то %.
        // Поэтому ошибка в теле действия перд чтением body (превышение размера, авторизация) будет не на 0%.
        // На малых файлах ошибка будет при 100%, что дает ложную информацию, что файл все таки загружен. 
        this.progress = undefined;

        this.message = 'Загрузка прервана!';
        this.messageStatus = 'error';

        const config: MatSnackBarConfig = {
          verticalPosition: 'top',
          panelClass: ['snackbar', 'error'],
          data: errorResponse
        };
        this.snackBar.openFromComponent(ErrorComponent, config);
      }, () => this.reset());
    }
  }

  cancel(): void {
    this.message = 'Загрузка отменена!';
    this.messageStatus = 'error';
    this.reset();
  }

  reset(): void {
    this.sub?.unsubscribe();
    this.sub = undefined;
    this.progress = undefined;
  }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
    this.reset();
  }
}
