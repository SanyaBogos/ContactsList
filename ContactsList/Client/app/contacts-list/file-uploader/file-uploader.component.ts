import { Component, Input, AfterViewInit } from '@angular/core';
import { NgModel, DefaultValueAccessor, NgControl } from '@angular/forms';
import { Http, Headers, RequestOptions } from '@angular/http';

@Component({
  selector: 'appc-file-uploader',
  templateUrl: './file-uploader.component.html',
  styleUrls: ['./file-uploader.component.scss'],
  providers: [NgModel, DefaultValueAccessor]
})
export class FileUploaderComponent implements AfterViewInit {

  static ROOT = '/rest/asset';

  @Input() private companyId: string = '';
  private value: string;
  private changeListener: Function;

  constructor(private http: Http, private input: NgControl) {
    this.input.valueAccessor = this;
  }

  ngAfterViewInit() {
  }

  writeValue(obj: any): void {
    this.value = obj;
  }

  registerOnChange(fn: any): void {
    this.changeListener = fn;
  }

  registerOnTouched(fn: any): void {

  }

  updated($event: any) {
    const files = $event.target.files || $event.srcElement.files;
    const file = files[0];
    const formData = new FormData();
    formData.append('file', file);

    const headers = new Headers({});
    let options = new RequestOptions({ headers });
    let url = FileUploaderComponent.ROOT + (this.companyId ? '/' + this.companyId : '');

    this.http.post(url, formData, options).subscribe(res => {
      let body = res.json();
      this.value = body.filename;

      if (this.changeListener) {
        this.changeListener(this.value);
      }
    });
  }
}

