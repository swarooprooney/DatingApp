import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from '../../_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { AuthService } from 'src/app/_services/auth.service';
import { environment } from 'src/environments/environment';
import { of } from 'rxjs';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css'],
})
export class PhotoEditorComponent implements OnInit {
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  response: string;
  baseUrl = environment.apiUrl;
  currentMainPhoto: Photo;
  @Input() photos: Photo[];
  @Output() getMemberPhotoChange = new EventEmitter<string>();
  constructor(
    private authservice: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }
  initializeUploader() {
    this.uploader = new FileUploader({
      url:
        this.baseUrl +
        'users/' +
        this.authservice.decodedToken.nameid +
        '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });
    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          dateAdded: res.dateAdded,
          url: res.url,
          isMain: res.isMain,
          description: res.description,
        };
        this.photos.push(photo);
      }
    };
  }
  setMainPhoto(photo: Photo) {
    this.userService
      .setMainPhoto(this.authservice.decodedToken.nameid, photo.id)
      .subscribe(
        () => {
         this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
         this.currentMainPhoto.isMain = false;
         photo.isMain = true;
         this.authservice.changeMemberPhoto(photo.url);
         this.authservice.currentUser.photoUrl = photo.url;
         localStorage.setItem('user', JSON.stringify(this.authservice.currentUser));
         this.alertify.success('Photo set to main successfully');
        },
        (error) => {
          this.alertify.error(error);
        }
      );
  }

  deletePhoto(id: number) {
    this.alertify.confirm('Are you sure you want to delete the photo?', () => {
      this.userService.deletePhoto(this.authservice.decodedToken.nameid, id)
    .subscribe(() => {
      this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
      this.alertify.success('Photo Deleted Successfully');
    }, error => {
      this.alertify.error(error);
    });
  });

  }
}
