import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { UploadService } from "../upload.service";
import { FileClient, FilesViewModel, ComplexResultViewModel } from "../../apiDefinitions";
import { FileUploader } from 'ng2-file-upload';

@Component({
  selector: 'appc-contacts-list',
  templateUrl: './contacts-list.component.html',
  styleUrls: ['./contacts-list.component.scss'],
  providers: [UploadService, FileClient]
})
export class ContactsListComponent implements OnInit {

  @ViewChild("fileInput") fileInput: ElementRef;

  public uploader: FileUploader;
  private complexResult: ComplexResultViewModel;
  private errors: string[];
  private warnings: string[];

  constructor(
    public uploadService: UploadService,
    public fileClient: FileClient,
  ) {
    this.uploader = new FileUploader({ url: `${window.location.origin}` });
  }

  ngOnInit() {
  }

  uploadFile() {
    let self = this;
    let fileNames: string[] = [];
    let sizes: number[] = [];
    let estimationTime = this.estimateUploadingTime(sizes.reduce((a, b) => a + b, 0));

    self.uploader.queue.forEach(x => {
      fileNames.push(x._file.name);
      sizes.push(x._file.size);
    });

    let filesVM = new FilesViewModel();
    filesVM.fileNames = fileNames;

    self.uploader.uploadAll();

    let interval = setInterval(function () {
      if (!self.uploader.isUploading) {
        self.dropPrevResults(self);

        self.fileClient.saveContacts(filesVM)
          .subscribe(data => {
            self.complexResult = data;

            if (self.complexResult.errorMessages)
              self.errors = self.complexResult.errorMessages;

            if (self.complexResult.warningMessages)
              self.warnings = self.complexResult.warningMessages;

            console.log(data);
          },
          err => {
            self.errors = [err];
            console.log(err);
          });
        clearInterval(interval);
      }
    }, estimationTime);
  }

  dropPrevResults(self: any) {
    self.errors = [];
    self.warnings = [];
    if (self.complexResult) {
      self.complexResult.successfullySaved = [];
      self.complexResult.issuedElements = [];
    }
  }

  isSuccessTableVisible() {
    return this.complexResult && this.complexResult.successfullySaved
      && (this.complexResult.successfullySaved.length > 0);
  }

  isIssuedTableVisible() {
    return this.complexResult && this.complexResult.issuedElements
      && (this.complexResult.issuedElements.length > 0);
  }

  isWarnings() {
    return this.complexResult && this.complexResult.warningMessages
      && (this.complexResult.warningMessages.length > 0);
  }

  private estimateUploadingTime(sizes: number): number {
    var time = sizes * 40 / 1073565696 / 2;
    return time > 5 ? 5 : time;
  }

}
